using Game.Core;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Game
{
    public class CostButton : MonoBehaviour
    {
        [HideInInspector] public UnityEvent OnClick = new();
        public Cost Cost => _cost;

        [SerializeField] private CurrenciesSettingsSO _currenciesSettings;
        [SerializeField] private Cost _cost;
        [SerializeField] private Image _costIcon;
        [SerializeField] private Text _costText;

        private Button _button;
        private bool _isBlocked = false;

        private void Awake()
        {
            _button = GetComponent<Button>();
            _button.onClick.AddListener(OnClick.Invoke);

            _costIcon.sprite = _currenciesSettings.GetCurrencyIconByType(_cost.Type);
            _costText.text = _cost.Value.ToString();

            CashManager.OnCurrencyUpdated.AddListener(RefreshInteractableOnCurrencyUpdated);

            SetInteractable(IsResourceEnought());
        }

        public void SetInteractable(bool isInteractable)
        {
            if (_isBlocked) return;
            if (isInteractable && IsResourceEnought() == false) return;

            _button.interactable = isInteractable;
        }

        public void SetCost(Currencies type, float value)
        {
            _cost.Type = type;
            _cost.Value = value;

            _costIcon.sprite = _currenciesSettings.GetCurrencyIconByType(_cost.Type);
            _costText.text = _cost.Value.ToString();

            SetInteractable(IsResourceEnought());
        }

        public void SetBlock(bool isBlocked)
        {
            _isBlocked = isBlocked;
        }

        private void RefreshInteractableOnCurrencyUpdated(float cashAmount, float cashDeltaChange, Currencies currency)
        {
            if (currency != Cost.Type) return;

            SetInteractable(cashAmount >= _cost.Value);
        }

        private bool IsResourceEnought()
        {
            return CashManager.Instance.IsEnough(_cost.Value, _cost.Type);
        }
    }
}