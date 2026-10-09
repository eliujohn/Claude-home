using UnityEngine;

namespace Ascension
{
    /// <summary>Movement, aim and dash. Dash i-frames are policy-controlled by sponsors (Trickster removes them).</summary>
    public class PlayerController : MonoBehaviour
    {
        Player player;
        Camera cam;
        float dashTimer, dashCd;
        Vector2 dashDir;
        public Vector2 Aim { get; private set; } = Vector2.up;
        public bool IsDashing => dashTimer > 0f;
        public float DashReady => dashCd <= 0f ? 1f : 1f - dashCd / Mathf.Max(0.01f, player.Stats.dashCooldown);

        void Awake() => player = GetComponent<Player>();

        void Update()
        {
            if (cam == null) cam = Camera.main;
            UpdateAim();

            if (GameManager.InputLocked) { player.Body.linearVelocity = Vector2.zero; return; }

            dashCd -= Time.deltaTime;
            var st = player.Stats;

            if (IsDashing)
            {
                dashTimer -= Time.deltaTime;
                player.Body.linearVelocity = dashDir * st.dashSpeed;
                if (dashTimer <= 0f) player.Health.invulnerable = false;
                return;
            }

            Vector2 move = InputReader.Move;
            float speed = st.moveSpeed * player.Sponsors.MoveSpeedMultiplier;
            player.Body.linearVelocity = Vector2.Lerp(player.Body.linearVelocity, move * speed, 1f - Mathf.Exp(-22f * Time.deltaTime));

            if (InputReader.DashPressed && dashCd <= 0f)
            {
                dashDir = move.sqrMagnitude > 0.01f ? move : Aim;
                dashTimer = st.dashTime;
                dashCd = st.dashCooldown;
                if (player.Sponsors.DashGivesIFrames) player.Health.invulnerable = true;
                player.Sponsors.NotifyDash();
                Fx.Pulse(player.Position, 0.7f, new Color(1, 1, 1, 0.4f), 0.2f);
            }
        }

        public void ResetDash() => dashCd = 0f;

        void UpdateAim()
        {
            if (cam == null) return;
            Vector3 w = cam.ScreenToWorldPoint(new Vector3(InputReader.MouseScreen.x, InputReader.MouseScreen.y, -cam.transform.position.z));
            Vector2 d = (Vector2)w - player.Position;
            if (d.sqrMagnitude > 0.01f) Aim = d.normalized;
        }

        public Vector2 AimPoint
        {
            get
            {
                if (cam == null) return player.Position + Aim * 5f;
                Vector3 w = cam.ScreenToWorldPoint(new Vector3(InputReader.MouseScreen.x, InputReader.MouseScreen.y, -cam.transform.position.z));
                return w;
            }
        }
    }
}
