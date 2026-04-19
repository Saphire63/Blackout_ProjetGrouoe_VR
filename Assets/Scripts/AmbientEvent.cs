using System.Collections;
using UnityEngine;

/// <summary>
/// Déclenche des événements ambiants aléatoires quand la bougie s'éteint.
/// Chaque événement combine un son + un objet qui bouge brièvement.
///
/// SETUP :
/// - Attacher sur un GameObject vide "AmbientEvents"
/// - Créer plusieurs AmbientEventData dans le tableau events[]
/// - Chaque event a un son + un objet de la scène qui va bouger
/// </summary>
public class AmbientEvent : MonoBehaviour
{
    [System.Serializable]
    public class AmbientEventData
    {
        [Tooltip("Nom de l'événement (juste pour s'y retrouver dans l'Inspector)")]
        public string eventName;

        [Header("Son")]
        public AudioClip sound;
        [Range(0f, 1f)]
        public float volume = 1f;

        [Header("Objet qui bouge")]
        [Tooltip("L'objet qui va se déplacer/trembler")]
        public GameObject movingObject;
        public EventMovementType movementType = EventMovementType.Shake;

        [Header("Mouvement")]
        [Tooltip("Intensité du mouvement")]
        public float intensity = 0.05f;
        [Tooltip("Durée du mouvement en secondes")]
        public float duration = 0.8f;

        [Header("Dialogue optionnel")]
        [Tooltip("Laisser vide pour ne pas afficher de dialogue")]
        public string dialogue = "";
    }

    public enum EventMovementType
    {
        Shake,      // tremblement sur place (cadre, tableau, verre)
        Fall,       // tombe légèrement et se redresse (objet sur table)
        Slide,      // glisse légèrement (chaise, livre)
    }

    [Header("Événements disponibles")]
    public AmbientEventData[] events;

    [Header("Audio")]
    public AudioSource audioSource;

    [Header("Délai avant l'événement")]
    [Tooltip("Secondes après l'extinction avant que l'événement se déclenche")]
    public float delayBeforeEvent = 1.5f;

    // ─── API publique ────────────────────────────────────────

    public void TriggerRandomEvent()
    {
        if (events == null || events.Length == 0) return;

        // Choisir un événement aléatoire
        int index = Random.Range(0, events.Length);
        StartCoroutine(PlayEvent(events[index]));
    }

    public void TriggerEvent(int index)
    {
        if (index < 0 || index >= events.Length) return;
        StartCoroutine(PlayEvent(events[index]));
    }

    // ─── Routine d'événement ─────────────────────────────────

    IEnumerator PlayEvent(AmbientEventData e)
    {
        yield return new WaitForSeconds(delayBeforeEvent);

        // Jouer le son
        if (audioSource && e.sound)
            audioSource.PlayOneShot(e.sound, e.volume);

        // Bouger l'objet
        if (e.movingObject != null)
        {
            switch (e.movementType)
            {
                case EventMovementType.Shake:
                    yield return StartCoroutine(ShakeObject(e.movingObject, e.intensity, e.duration));
                    break;
                case EventMovementType.Fall:
                    yield return StartCoroutine(FallObject(e.movingObject, e.intensity, e.duration));
                    break;
                case EventMovementType.Slide:
                    yield return StartCoroutine(SlideObject(e.movingObject, e.intensity, e.duration));
                    break;
            }
        }

        // Dialogue optionnel
        if (!string.IsNullOrEmpty(e.dialogue))
            GameManager.Instance.dialogueSystem.ShowDialogue(e.dialogue, 3f, null);
    }

    // ─── Types de mouvement ──────────────────────────────────

    IEnumerator ShakeObject(GameObject obj, float intensity, float duration)
    {
        // Tremblement aléatoire sur place (cadre, tableau, verre sur table)
        Vector3 originalPos = obj.transform.localPosition;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;

            // Intensité qui diminue progressivement
            float currentIntensity = intensity * (1f - progress);

            obj.transform.localPosition = originalPos + new Vector3(
                Random.Range(-currentIntensity, currentIntensity),
                Random.Range(-currentIntensity * 0.3f, currentIntensity * 0.3f),
                Random.Range(-currentIntensity, currentIntensity)
            );

            yield return null;
        }

        obj.transform.localPosition = originalPos;
    }

    IEnumerator FallObject(GameObject obj, float intensity, float duration)
    {
        // L'objet penche et revient (livre, vase, bibelot)
        Quaternion originalRot = obj.transform.localRotation;
        Vector3 originalPos = obj.transform.localPosition;

        float halfDuration = duration * 0.5f;
        float tiltAngle = intensity * 20f; // convertir en degrés

        // Phase 1 : pencher
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / halfDuration;
            obj.transform.localRotation = Quaternion.Lerp(
                originalRot,
                originalRot * Quaternion.Euler(tiltAngle, 0f, tiltAngle * 0.5f),
                Mathf.SmoothStep(0f, 1f, t)
            );
            yield return null;
        }

        // Phase 2 : revenir avec un rebond
        t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / halfDuration;
            float bounce = Mathf.Sin(t * Mathf.PI * 3f) * (1f - t) * 0.3f;
            obj.transform.localRotation = Quaternion.Lerp(
                originalRot * Quaternion.Euler(tiltAngle, 0f, tiltAngle * 0.5f),
                originalRot,
                Mathf.SmoothStep(0f, 1f, t)
            );
            obj.transform.localPosition = originalPos + Vector3.up * bounce * intensity;
            yield return null;
        }

        obj.transform.localRotation = originalRot;
        obj.transform.localPosition = originalPos;
    }

    IEnumerator SlideObject(GameObject obj, float intensity, float duration)
    {
        // L'objet glisse légèrement dans une direction (chaise, livre sur table)
        Vector3 originalPos = obj.transform.localPosition;
        Vector3 slideDir = new Vector3(
            Random.Range(-1f, 1f),
            0f,
            Random.Range(-1f, 1f)
        ).normalized;

        Vector3 targetPos = originalPos + slideDir * intensity * 0.2f;

        // Glisser vers la position cible
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / (duration * 0.4f);
            obj.transform.localPosition = Vector3.Lerp(originalPos, targetPos, Mathf.SmoothStep(0f, 1f, t));
            yield return null;
        }

        // Petits tremblements à l'arrêt
        yield return StartCoroutine(ShakeObject(obj, intensity * 0.3f, duration * 0.3f));

        // L'objet reste dans sa nouvelle position (ne revient pas — plus réaliste)
        // Si tu veux qu'il revienne, décommente la ligne suivante :
        // obj.transform.localPosition = originalPos;
    }
}
