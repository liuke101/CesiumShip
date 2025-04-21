using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Lovatto.MiniMap
{
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(AudioSource))]
    public class MinimapPlayerController : MonoBehaviour
    {
        public bool is2D = false;
        public bool m_IsWalking;
        public float m_WalkSpeed;
        public float m_RunSpeed;
        [Range(0f, 1f)] public float m_RunstepLenghten;
        public float m_JumpSpeed;
        public float m_StickToGroundForce;
        public float m_GravityMultiplier;
        public bl_MouseLook m_MouseLook;
        public bool m_UseFovKick;
        public bl_FOVKick m_FovKick = new bl_FOVKick();
        public bool m_UseHeadBob;
        public bl_CurveControlledBob m_HeadBob = new bl_CurveControlledBob();
        public bl_LerpControlledBob m_JumpBob = new bl_LerpControlledBob();
        public float m_StepInterval;
        public AudioClip[] m_FootstepSounds;    // an array of footstep sounds that will be randomly selected from.
        public AudioClip m_JumpSound;           // the sound played when character leaves the ground.
        public AudioClip m_LandSound;           // the sound played when character touches back on ground.

        private Camera m_Camera;
        private bool m_Jump;
        private Vector2 m_Input;
        private Vector3 m_MoveDir = Vector3.zero;
        private CharacterController m_CharacterController;
        private CollisionFlags m_CollisionFlags;
        private bool m_PreviouslyGrounded;
        private Vector3 m_OriginalCameraPosition;
        private float m_StepCycle;
        private float m_NextStep;
        private bool m_Jumping;
        private AudioSource m_AudioSource;
        private float targetSpeed = 4;

        // Use this for initialization
        private void Start()
        {
            m_CharacterController = GetComponent<CharacterController>();
            m_Camera = Camera.main;
            m_OriginalCameraPosition = m_Camera.transform.localPosition;

            if (!is2D)
            {
                m_FovKick.Setup(m_Camera);
                m_HeadBob.Setup(m_Camera, m_StepInterval);
            }
            m_StepCycle = 0f;
            m_NextStep = m_StepCycle / 2f;
            m_Jumping = false;
            m_AudioSource = GetComponent<AudioSource>();
            if (!is2D)
            {
                m_MouseLook.Init(transform, m_Camera.transform);
            }
        }

        private void Update()
        {
            if (bl_MiniMap.ActiveMiniMap != null && bl_MiniMap.ActiveMiniMap.IsFullScreen) return;

            // the jump state needs to read here to make sure it is not missed
            if (!m_Jump)
            {
                m_Jump = Input.GetKeyDown(KeyCode.Space);
            }

            if (!m_PreviouslyGrounded && m_CharacterController.isGrounded)
            {
                StartCoroutine(m_JumpBob.DoBobCycle());
                PlayLandingSound();
                m_MoveDir.y = 0f;
                m_Jumping = false;
            }
            if (!m_CharacterController.isGrounded && !m_Jumping && m_PreviouslyGrounded)
            {
                m_MoveDir.y = 0f;
            }

        }

        private void PlayLandingSound()
        {
            m_AudioSource.clip = m_LandSound;
            m_AudioSource.Play();
            m_NextStep = m_StepCycle + .5f;
        }

        private void FixedUpdate()
        {
            GetInput(out targetSpeed);
            // always move along the camera forward as it is the direction that it being aimed at
            Vector3 desiredMove = transform.forward * m_Input.y + transform.right * m_Input.x;

            // get a normal for the surface that is being touched to move along it
            RaycastHit hitInfo;
            Physics.SphereCast(transform.position, m_CharacterController.radius, Vector3.down, out hitInfo,
                               m_CharacterController.height / 2f);
            desiredMove = Vector3.ProjectOnPlane(desiredMove, hitInfo.normal).normalized;

            m_MoveDir.x = desiredMove.x * targetSpeed;
            if (!is2D)
            {
                m_MoveDir.z = desiredMove.z * targetSpeed;
            }


            if (m_CharacterController.isGrounded)
            {
                m_MoveDir.y = -m_StickToGroundForce;

                if (m_Jump)
                {
                    m_MoveDir.y = m_JumpSpeed;
                    PlayJumpSound();
                    m_Jump = false;
                    m_Jumping = true;
                }
            }
            else
            {
                m_MoveDir += m_GravityMultiplier * Time.fixedDeltaTime * Physics.gravity;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void LateUpdate()
        {
            if (bl_MiniMap.ActiveMiniMap != null && bl_MiniMap.ActiveMiniMap.IsFullScreen) return;

            if (!is2D)
            {
                RotateView();
            }

            m_CollisionFlags = m_CharacterController.Move(m_MoveDir * Time.fixedDeltaTime);

            ProgressStepCycle(targetSpeed);
            UpdateCameraPosition(targetSpeed);

            m_PreviouslyGrounded = m_CharacterController.isGrounded;
        }

        private void PlayJumpSound()
        {
            m_AudioSource.clip = m_JumpSound;
            m_AudioSource.Play();
        }

        private void ProgressStepCycle(float speed)
        {
            if (m_CharacterController.velocity.sqrMagnitude > 0 && (m_Input.x != 0 || m_Input.y != 0))
            {
                m_StepCycle += (m_CharacterController.velocity.magnitude + (speed * (m_IsWalking ? 1f : m_RunstepLenghten))) *
                             Time.fixedDeltaTime;
            }

            if (!(m_StepCycle > m_NextStep))
            {
                return;
            }

            m_NextStep = m_StepCycle + m_StepInterval;

            PlayFootStepAudio();
        }

        private void PlayFootStepAudio()
        {
            if (!m_CharacterController.isGrounded)
            {
                return;
            }
            // pick & play a random footstep sound from the array,
            // excluding sound at index 0
            int n = Random.Range(1, m_FootstepSounds.Length);
            m_AudioSource.clip = m_FootstepSounds[n];
            m_AudioSource.PlayOneShot(m_AudioSource.clip);
            // move picked sound to index 0 so it's not picked next time
            m_FootstepSounds[n] = m_FootstepSounds[0];
            m_FootstepSounds[0] = m_AudioSource.clip;
        }

        private void UpdateCameraPosition(float speed)
        {
            if (!is2D)
            {
                Vector3 newCameraPosition;
                if (!m_UseHeadBob)
                {
                    return;
                }
                if (m_CharacterController.velocity.magnitude > 0 && m_CharacterController.isGrounded)
                {
                    m_Camera.transform.localPosition =
                        m_HeadBob.DoHeadBob(m_CharacterController.velocity.magnitude +
                                          (speed * (m_IsWalking ? 1f : m_RunstepLenghten)));
                    newCameraPosition = m_Camera.transform.localPosition;
                    newCameraPosition.y = m_Camera.transform.localPosition.y - m_JumpBob.Offset();
                }
                else
                {
                    newCameraPosition = m_Camera.transform.localPosition;
                    newCameraPosition.y = m_OriginalCameraPosition.y - m_JumpBob.Offset();
                }
                m_Camera.transform.localPosition = newCameraPosition;
            }
            else
            {
                Vector3 p = transform.position;
                m_Camera.transform.localPosition = Vector3.Lerp(m_Camera.transform.localPosition, new Vector3(p.x, p.y, m_OriginalCameraPosition.z), Time.deltaTime * 7);
            }
        }


        private void GetInput(out float speed)
        {
            // Read input
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");

            bool waswalking = m_IsWalking;

#if !MOBILE_INPUT
            // On standalone builds, walk/run speed is modified by a key press.
            // keep track of whether or not the character is walking or running
            m_IsWalking = !Input.GetKey(KeyCode.LeftShift);
#endif
            // set the desired speed to be walking or running
            speed = m_IsWalking ? m_WalkSpeed : m_RunSpeed;
            m_Input = new Vector2(horizontal, vertical);

            // handle speed change to give an fov kick
            // only if the player is going to a run, is running and the fovkick is to be used
            if (m_IsWalking != waswalking && m_UseFovKick && m_CharacterController.velocity.sqrMagnitude > 0)
            {
                StopAllCoroutines();
                StartCoroutine(!m_IsWalking ? m_FovKick.FOVKickUp() : m_FovKick.FOVKickDown());
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void RotateView()
        {
            m_MouseLook.LookRotation(transform, m_Camera.transform);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="hit"></param>
        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            Rigidbody body = hit.collider.attachedRigidbody;
            //dont move the rigidbody if the character is on top of it
            if (m_CollisionFlags == CollisionFlags.Below)
            {
                return;
            }

            if (body == null || body.isKinematic)
            {
                return;
            }
            body.AddForceAtPosition(m_CharacterController.velocity * 0.1f, hit.point, ForceMode.Impulse);
        }
    }
}