using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PizzaStackVisual : MonoBehaviour
{
    [Header("Stack-Verbindungen")]
    [SerializeField, Tooltip("Der Anker-Transform, unter dem der Stapel aufgebaut wird (Helper_PizzaBox).")]
    private Transform stackAnchor;

    [SerializeField, Tooltip("Die vorhandene pizzav3-Box; definiert die Basispose für den Stapel.")]
    private Transform referenceBox;

    [Header("Prefabs")]
    [SerializeField, Tooltip("Das Prefab für die geschlossene Pizzaschachtel (pizzav3).")]
    private GameObject pizzaBoxPrefab;

    [SerializeField, Tooltip("Das Prefab für die offene Pizza ganz oben (pizzafull).")]
    private GameObject topPizzaPrefab;

    [Header("Lebenssystem")]
    [SerializeField, Tooltip("Optionale Referenz auf das PlayerLifeSystem. Wird sonst automatisch ermittelt.")]
    private PlayerLifeSystem lifeSystem;

    [Header("Stapel-Einstellungen")]
    [SerializeField, Tooltip("Wie dicht die Schachteln aufeinander liegen (1.0 = berühren sich genau).")]
    private float stackGap = 1.0f;

    [SerializeField, Tooltip("Maximale zufällige Neigung um die Hochachse (in Grad).")]
    private float maxLeanAngle = 6f;

    [SerializeField, Tooltip("Korrektur-Drehwinkel für die geöffnete Pizza, damit die Öffnung zur Kamera zeigt.")]
    private float openBoxYawOffset = 0f;

    [SerializeField, Tooltip("Verschiebung der offenen Pizza nach oben entlang der Stapelachse.")]
    private float openBoxStackOffset = 0.104f;

    [SerializeField, Tooltip("Maximale zufällige seitliche Verschiebung pro Schachtel.")]
    private float maxHorizontalOffset = 0.01f;

    [SerializeField, Tooltip("Größenmultiplikator für alle Schachteln (0.9 = 10% kleiner).")]
    private float boxScaleMultiplier = 0.9f;

    [SerializeField, Tooltip("Dauer der Hinzufügen- oder Entfernen-Animation (in Sekunden).")]
    private float animDuration = 0.5f;

    private Vector3 baseLocalPosition = Vector3.zero;
    private Quaternion baseLocalRotation = Quaternion.identity;
    private Vector3 baseLocalScale = Vector3.one;
    private Vector3 stackDir = Vector3.up;
    private float spacing = 0.05f;

    private Transform stackContainer;
    private List<GameObject> spawnedBoxes = new List<GameObject>();
    private Coroutine activeAnimCoroutine;
    private int lastLives;

    private void Awake()
    {
        // Erstelle einen Container-Transform unter dem Anker, um die gesamte Stapelbewegung separat zu animieren.
        if (stackAnchor != null)
        {
            GameObject containerObj = new GameObject("PizzaStackContainer");
            stackContainer = containerObj.transform;
            stackContainer.SetParent(stackAnchor);
            stackContainer.localPosition = Vector3.zero;
            stackContainer.localRotation = Quaternion.identity;
            stackContainer.localScale = Vector3.one;
        }
        else
        {
            stackContainer = transform;
        }

        // Erfasse die Basispose der Referenzschachtel, falls vorhanden.
        if (referenceBox != null)
        {
            baseLocalPosition = referenceBox.localPosition;
            baseLocalRotation = referenceBox.localRotation;
            baseLocalScale = referenceBox.localScale;
            referenceBox.gameObject.SetActive(false);
        }
        else
        {
            baseLocalPosition = Vector3.zero;
            baseLocalRotation = Quaternion.identity;
            if (pizzaBoxPrefab != null)
            {
                baseLocalScale = pizzaBoxPrefab.transform.localScale;
            }
            else
            {
                baseLocalScale = Vector3.one;
            }
        }

        // Die Stapelrichtung ist die eigene Hochachse der Schachtel im lokalen Raum des Ankers.
        stackDir = (baseLocalRotation * Vector3.up).normalized;

        // Bestimme die Schachteldicke dynamisch
        if (pizzaBoxPrefab != null)
        {
            GameObject tempInstance = Instantiate(pizzaBoxPrefab, Vector3.zero, Quaternion.identity);
            tempInstance.transform.localScale = baseLocalScale;
            Bounds combinedBounds = new Bounds();
            Renderer[] renderers = tempInstance.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length > 0)
            {
                combinedBounds = renderers[0].bounds;
                for (int j = 1; j < renderers.Length; j++)
                {
                    combinedBounds.Encapsulate(renderers[j].bounds);
                }
                float thickness = Mathf.Min(combinedBounds.size.x, Mathf.Min(combinedBounds.size.y, combinedBounds.size.z));
                spacing = thickness * stackGap;
            }
            else
            {
                spacing = 0.05f * stackGap; // Fallback
            }
            DestroyImmediate(tempInstance);
        }
        else
        {
            spacing = 0.05f * stackGap;
        }

        // Skaliere den Abstand um den Größenmultiplikator der Schachteln
        spacing *= boxScaleMultiplier;
    }

    private void OnEnable()
    {
        if (lifeSystem == null)
        {
            lifeSystem = GetComponentInParent<PlayerLifeSystem>();
            if (lifeSystem == null)
            {
                lifeSystem = FindFirstObjectByType<PlayerLifeSystem>();
            }
        }

        if (lifeSystem != null)
        {
            lifeSystem.OnLivesChanged += HandleLivesChanged;
            lastLives = lifeSystem.CurrentLives;
            RebuildStackInstant(lastLives);
        }
        else
        {
            Debug.LogWarning("[PizzaStackVisual] PlayerLifeSystem konnte nicht im Parent oder in der Szene gefunden werden.");
        }
    }

    private void OnDisable()
    {
        if (lifeSystem != null)
        {
            lifeSystem.OnLivesChanged -= HandleLivesChanged;
        }
    }

    private void OnDestroy()
    {
        if (lifeSystem != null)
        {
            lifeSystem.OnLivesChanged -= HandleLivesChanged;
        }
    }

    private void HandleLivesChanged(int currentLives, int maxLives)
    {
        if (activeAnimCoroutine != null)
        {
            StopCoroutine(activeAnimCoroutine);
            activeAnimCoroutine = null;
            // Snap to previous state before applying the new delta
            RebuildStackInstant(lastLives);
        }

        int oldLives = lastLives;
        lastLives = currentLives;

        if (currentLives == oldLives) return;

        // Wenn ein Leben hinzugefügt wurde (+1)
        if (currentLives == oldLives + 1)
        {
            RebuildStackInstant(currentLives);

            if (spawnedBoxes.Count > 0)
            {
                GameObject newTop = spawnedBoxes[spawnedBoxes.Count - 1];
                activeAnimCoroutine = StartCoroutine(AddLifeCoroutine(newTop));
            }
        }
        // Wenn ein Leben verloren ging (-1)
        else if (currentLives == oldLives - 1)
        {
            GameObject fallingBox = null;
            if (spawnedBoxes.Count > 0)
            {
                fallingBox = spawnedBoxes[spawnedBoxes.Count - 1];
                spawnedBoxes.RemoveAt(spawnedBoxes.Count - 1);
                fallingBox.transform.SetParent(null); // Vom Stapel trennen
            }

            RebuildStackInstant(currentLives);

            if (fallingBox != null)
            {
                StartCoroutine(AnimateFallingBox(fallingBox));
            }
        }
        // Bei größerer Differenz (z.B. Zurücksetzen oder Initialisierung) ohne Animation neu aufbauen
        else
        {
            RebuildStackInstant(currentLives);
        }
    }

    private void RebuildStackInstant(int lives)
    {
        foreach (var box in spawnedBoxes)
        {
            if (box != null)
            {
                Destroy(box);
            }
        }
        spawnedBoxes.Clear();

        if (stackContainer != null)
        {
            stackContainer.localPosition = Vector3.zero;
        }

        for (int i = 0; i < lives; i++)
        {
            GameObject prefab = (i == lives - 1) ? topPizzaPrefab : pizzaBoxPrefab;
            if (prefab == null) continue;

            GameObject box = Instantiate(prefab, stackContainer);
            CalculateSlotPose(i, lives, out Vector3 localPos, out Quaternion localRot, out Vector3 localScale);

            box.transform.localPosition = localPos;
            box.transform.localRotation = localRot;
            box.transform.localScale = localScale;

            spawnedBoxes.Add(box);
        }
    }

    private void CalculateSlotPose(int i, int totalLives, out Vector3 localPos, out Quaternion localRot, out Vector3 localScale)
    {
        localScale = baseLocalScale * boxScaleMultiplier;

        // Deterministischer Zufallsgenerator pro Index i, damit der Stapel stabil bleibt
        System.Random rand = new System.Random(i + 1337);

        // Verschiebung entlang der Stapelachse
        localPos = baseLocalPosition + stackDir * (i * spacing);

        if (i == totalLives - 1)
        {
            // Verschiebung der geöffneten Schachtel nach oben entlang der Stapelachse (ebenfalls skaliert)
            float effectiveOpenOffset = openBoxStackOffset * boxScaleMultiplier;
            localPos += stackDir * effectiveOpenOffset;

            // Topbox: Geöffneter Karton zeigt zur Kamera (keine zufällige Neigung)
            Vector3 worldBoxPos = stackAnchor != null ? stackAnchor.TransformPoint(localPos) : localPos;
            Transform cam = Camera.main != null ? Camera.main.transform : null;
            Vector3 dirToCam = cam != null ? (cam.position - worldBoxPos) : -Vector3.forward;

            Vector3 worldStackDir = stackAnchor != null ? stackAnchor.TransformDirection(stackDir) : stackDir;
            Vector3 worldBaseRotationForward = (stackAnchor != null ? stackAnchor.rotation : Quaternion.identity) * baseLocalRotation * Vector3.forward;

            Vector3 projectedDir = Vector3.ProjectOnPlane(dirToCam, worldStackDir).normalized;
            if (projectedDir == Vector3.zero)
            {
                projectedDir = Vector3.ProjectOnPlane(-Vector3.forward, worldStackDir).normalized;
            }
            if (projectedDir == Vector3.zero)
            {
                projectedDir = Vector3.forward;
            }

            Vector3 refForward = Vector3.ProjectOnPlane(worldBaseRotationForward, worldStackDir).normalized;
            if (refForward == Vector3.zero)
            {
                refForward = Vector3.forward;
            }

            float angleToCam = Vector3.SignedAngle(refForward, projectedDir, worldStackDir);
            float finalYaw = angleToCam + openBoxYawOffset;

            localRot = Quaternion.AngleAxis(finalYaw, stackDir) * baseLocalRotation;
        }
        else
        {
            // Untere pizzav3 Schachteln erhalten eine kleine zufällige Drehung um die Hochachse
            float leanAngle = (float)(rand.NextDouble() * 2.0 - 1.0) * maxLeanAngle;
            localRot = Quaternion.AngleAxis(leanAngle, Vector3.up) * baseLocalRotation;

            // Kleiner seitlicher Offset senkrecht zur Stapelachse
            Vector3 u = Vector3.right;
            Vector3 v = Vector3.forward;
            Vector3.OrthoNormalize(ref stackDir, ref u, ref v);

            float offsetX = (float)(rand.NextDouble() * 2.0 - 1.0) * maxHorizontalOffset;
            float offsetZ = (float)(rand.NextDouble() * 2.0 - 1.0) * maxHorizontalOffset;
            Vector3 randomOffset = u * offsetX + v * offsetZ;

            localPos += randomOffset;
        }
    }

    private IEnumerator AddLifeCoroutine(GameObject newTopBox)
    {
        float elapsed = 0f;
        Vector3 targetScale = baseLocalScale * boxScaleMultiplier;

        if (stackContainer != null)
        {
            stackContainer.localPosition = Vector3.zero;
        }

        while (elapsed < animDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / animDuration);

            // Pop-Up Effekt mit Überlauf-Dämpfung (Overshoot)
            if (newTopBox != null)
            {
                float s = EaseOutBack(t);
                newTopBox.transform.localScale = targetScale * s;
            }

            // Minimales Aufhüpfen des gesamten Stapels
            if (stackContainer != null)
            {
                float hopHeight = spacing * 0.4f;
                float hop = Mathf.Sin(t * Mathf.PI) * hopHeight;
                stackContainer.localPosition = stackDir * hop;
            }

            yield return null;
        }

        if (newTopBox != null)
        {
            newTopBox.transform.localScale = targetScale;
        }
        if (stackContainer != null)
        {
            stackContainer.localPosition = Vector3.zero;
        }
    }

    private IEnumerator AnimateFallingBox(GameObject box)
    {
        if (box == null) yield break;

        float elapsed = 0f;
        Vector3 originalScale = box.transform.localScale;

        // Berechne Anfangsimpuls im Weltraum relativ zum Anker
        Vector3 upDir = stackAnchor != null ? stackAnchor.up : Vector3.up;
        Vector3 rightDir = stackAnchor != null ? stackAnchor.right : Vector3.right;

        Vector3 randomDir = Random.onUnitSphere;
        randomDir.y = Mathf.Abs(randomDir.y); // Bevorzuge Aufwärtsrichtung

        Vector3 velocity = upDir * 1.5f + rightDir * Random.Range(-0.5f, 0.5f) + randomDir * 0.5f;

        Vector3 spinAxis = Random.onUnitSphere;
        float spinSpeed = Random.Range(360f, 720f);
        float gravity = 15f; // Etwas stärkere Schwerkraft für ein knackiges Abfallen

        while (elapsed < animDuration)
        {
            if (box == null) yield break;

            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / animDuration);

            // Physik-Schritt
            velocity += Vector3.down * gravity * Time.deltaTime;
            box.transform.position += velocity * Time.deltaTime;
            box.transform.Rotate(spinAxis, spinSpeed * Time.deltaTime, Space.World);

            // Skaliere am Ende der Flugkurve gegen Null
            box.transform.localScale = Vector3.Lerp(originalScale, Vector3.zero, t);

            yield return null;
        }

        if (box != null)
        {
            Destroy(box);
        }
    }

    private float EaseOutBack(float t)
    {
        float c1 = 1.70158f;
        float c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
    }
}

