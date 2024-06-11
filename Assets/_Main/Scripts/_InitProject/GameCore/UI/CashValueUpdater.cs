using Game.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI {
    /// <summary>
    /// Cash text value updater that auto updates on cash change
    /// </summary>
	[AddComponentMenu("Common/UI/Cash Value Updater")]
    public class CashValueUpdater : TextValueUpdater {
        [SerializeField] private CurrenciesSettingsSO _currencySettings;
        [SerializeField] private Currencies _currency = Currencies.Cash;
        [SerializeField] private Image _icon;

        protected override void Awake() {
            base.Awake();
            if (_icon) {
                _icon.sprite = _currencySettings.GetCurrencyIconByType(_currency);
            }
        }

        private void OnEnable() {
            SetStartValue(CashManager.Instance.GetAmount(_currency));
            CashManager.OnCurrencyUpdated.AddListener(UpdateCashUI);
        }

        private void OnDisable() {
            CashManager.OnCurrencyUpdated.RemoveListener(UpdateCashUI);
        }

        private void UpdateCashUI(float cash, float delta, Currencies currency) {
            if (_currency != currency) {
                return;
            }

            if (delta == 0) {
                UpdateTextInstant(cash);
                return;
            }
            UpdateTextSmooth(cash);
        }
    }
}

