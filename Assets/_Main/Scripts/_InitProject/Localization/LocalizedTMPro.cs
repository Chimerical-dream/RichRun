using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
namespace Localization
{
    public class LocalizedTMPro : TextMeshProUGUI
    {
        [SerializeField]
        public string textEn = null,
            textRu = null,
            textTr = null;

        public string LocalizedText
        {
            get
            {
                string text;
                if (Language.Lang == "ru" && textRu != null)
                {
                    text = textRu;
                }
                else if (Language.Lang == "tr" && textTr != null)
                {
                    text = textTr;
                }
                else //english
                {
                    text = textEn;
                }
                text = text.Replace("\\n", "\n");
                return text;
            }
        }

        protected override void Awake()
        {
            base.Awake();
            
            this.text = LocalizedText;
        }
    }
}
