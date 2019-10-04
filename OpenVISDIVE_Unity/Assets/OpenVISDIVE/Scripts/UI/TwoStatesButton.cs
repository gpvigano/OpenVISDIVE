using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace OpenVISDIVE
{
    [RequireComponent(typeof(Toggle))]
    public class TwoStatesButton : MonoBehaviour
    {
        [SerializeField]
        private Graphic onGraphic = null;
        [SerializeField]
        private Graphic offGraphic = null;
        [SerializeField]
        private Text onTooltipText = null;
        [SerializeField]
        private Text offTooltipText = null;
        private Toggle toggleButton = null;

        public void Sync(bool on)
        {
            onGraphic.gameObject.SetActive(on);
            offGraphic.gameObject.SetActive(!on);
            toggleButton.targetGraphic = on ? onGraphic : offGraphic;
            toggleButton.graphic = null;
            onTooltipText.gameObject.SetActive(on);
            offTooltipText.gameObject.SetActive(!on);
        }



        private void OnValidate()
        {
            Init();
        }


        private void Awake()
        {
            Init();
        }


        private void Init()
        {
            toggleButton = GetComponent<Toggle>();
            if (onGraphic == null)
            {
                onGraphic = toggleButton.graphic;
                onGraphic.gameObject.SetActive(toggleButton.isOn);
            }
            if (offGraphic == null)
            {
                offGraphic = toggleButton.targetGraphic;
                offGraphic.gameObject.SetActive(!toggleButton.isOn);
            }
            if (onTooltipText == null)
            {
                onTooltipText = onGraphic?.GetComponentInChildren<Text>();
            }
            if (offTooltipText == null)
            {
                offTooltipText = offGraphic?.GetComponentInChildren<Text>();
            }
        }


        private void Start()
        {
            Sync(toggleButton.isOn);
            toggleButton.onValueChanged.AddListener(Sync);
        }
    }
}
