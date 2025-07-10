using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

namespace Platformer.Gameplay
{
    /// <summary>
    /// </summary>

    public class TextMeshProLinkHandler : MonoBehaviour, IPointerClickHandler
    {
        private TextMeshProUGUI textMeshPro;
        private Camera uiCamera;

        void Start()
        {
            textMeshPro = GetComponent<TextMeshProUGUI>();
            // Get the camera that renders the Canvas
            uiCamera = GetComponentInParent<Canvas>().worldCamera;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            // Convert screen point to local point in RectTransform
            Vector3 mousePosition = new Vector3(eventData.position.x, eventData.position.y, 0);

            // Check if we clicked on a link
            int linkIndex = TMP_TextUtilities.FindIntersectingLink(textMeshPro, mousePosition, uiCamera);

            if (linkIndex != -1)
            {
                // Get link info
                TMP_LinkInfo linkInfo = textMeshPro.textInfo.linkInfo[linkIndex];
                string linkID = linkInfo.GetLinkID();

                // Handle the link click
                HandleLinkClick(linkID);
            }
        }

        private void HandleLinkClick(string linkID)
        {
            switch (linkID)
            {
                case "linkWeb":
                    Application.OpenURL("https://play.unity.com/en/games/3ce0aab3-e765-44c7-b360-549493a7014f/bank-escape");
                    break;
                case "linkDemoVideo":
                    Application.OpenURL("https://youtu.be/MfBgfzwalLA");
                    break;
                case "linkProjectFiles":
                    Application.OpenURL("https://github.com/eprael/BankRobber_2D_Unity");
                    break;
            }
        }
    }
}