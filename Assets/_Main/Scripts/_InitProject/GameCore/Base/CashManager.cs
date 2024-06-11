using NaughtyAttributes;
using System;
using System.Collections.Generic;
using UnityEngine.Events;

namespace Game.Core {
    [Serializable]
    public enum Currencies {
        Cash,
        Hard
    }

    [Serializable]
    public class Cost {
        public Currencies Type;
        public float Value;
    }

    public class CashManager : MonoSingleton<CashManager> {
        private SaveSystem _save;

        private Dictionary<Currencies, float> _currencies = new Dictionary<Currencies, float>();
        /// <summary>
        /// arg1 is new cash amount<br>
        /// arg2 is cash delta change</br>
        /// arg3 is what currency to update
        /// </summary>
        public static UnityEvent<float, float, Currencies> OnCurrencyUpdated = new();
        public static UnityEvent<Currencies> OnNotEnoughCurrency = new();

        [UnityEngine.SerializeField] private CurrenciesSettingsSO _currenciesSettings;
        private Currencies[] _allCurrencies;

        protected override void Awake() {
            base.Awake();
            if (_awakeDestroyObj) return;
            DontDestroyOnLoad(gameObject);
#if UNITY_EDITOR
            gameObject.name = "CashManager";
            if (_currenciesSettings == null) {
                _currenciesSettings = EditorTools.EditorAssetManager.LoadAsset<CurrenciesSettingsSO>("Currencies Settings", "Assets/_Main/Configs");
            }
#endif

            _save = SaveSystem.Instance;
            if (_save.Data.Currencies == null) {
                _save.Data.Currencies = _currenciesSettings.GenerateStartCurrencies();
            }
            _currencies = new Dictionary<Currencies, float>(_save.Data.Currencies);

            _allCurrencies = (Currencies[])Enum.GetValues(typeof(Currencies));
			var saveCurrencies = _save.Data.Currencies;
            foreach (var currency in _allCurrencies) {
                if (_currencies.ContainsKey(currency) == false) {
                    _currencies.Add(currency, 0);
                }
				
				if (saveCurrencies.ContainsKey(currency) == false) {
                    saveCurrencies.Add(currency, 0);
				}
            }
        }

        private void Start() {
            OnCurrencyUpdated.Invoke(_currencies[Currencies.Cash], 0, Currencies.Cash);
        }

        public float GetAmount(Currencies currency) {
            return _currencies[currency];
        }

        /// <summary>
        /// Write all currency values to save data
        /// </summary>
        public void SaveStates() {
            foreach (var currency in _allCurrencies) {
                _save.Data.Currencies[currency] = _currencies[currency];
            }
        }

        /// <summary>
        /// Back all currency values to save data values
        /// </summary>
        public void ResetStates() {
            foreach (var currency in _allCurrencies) {
                if (_currencies[currency] != _save.Data.Currencies[currency]) {
                    _currencies[currency] = _save.Data.Currencies[currency];
                    OnCurrencyUpdated.Invoke(_currencies[currency], 0f, currency);
                }
            }
        }

        /// <summary>
        /// Note: it doesnt write to save data, if you need use <see cref="AddAndSave(float, Currencies)"/>
        /// </summary>
        public void Add(float amount, Currencies currency) {
            _currencies[currency] += amount;
            OnCurrencyUpdated.Invoke(_currencies[currency], amount, currency);
        }

        /// <summary>
        /// Note: it doesnt write to save data, if you need use <see cref="AddAndSave(Cost)"/>
        /// </summary>
        public void Add(Cost cost) {
            Add(cost.Value, cost.Type);
        }

        public void AddAndSave(float amount, Currencies currency) {
            _currencies[currency] += amount;
            OnCurrencyUpdated.Invoke(_currencies[currency], amount, currency);
            _save.Data.Currencies[currency] = _currencies[currency];
        }

        public void AddAndSave(Cost cost) {
            AddAndSave(cost.Value, cost.Type);
        }

        /// <summary>
        /// Adds cash by currency, doesn't invoke update event (UI), but changes CashManager currency value
        /// </summary>
        /// <param name="withSave">also write transaction to save data so action will be saved</param>
        public void AddWithoutUpdateEvent(float amount, Currencies currency, bool withSave = true) {
            _currencies[currency] += amount;
            if (withSave) _save.Data.Currencies[currency] = _currencies[currency];
        }

        /// <summary>
        /// Adds cash by currency, doesn't invoke update event (UI), but changes CashManager currency value
        /// </summary>
        /// <param name="withSave">also write transaction to save data so action will be saved</param>
        public void AddWithoutUpdateEvent(Cost cost, bool withSave = true) {
            AddWithoutUpdateEvent(cost.Value, cost.Type, withSave);
        }

        /// <summary>
        /// Note: it doesnt write to save data, if you need use <see cref="SpendAndSave(float, Currencies)"/>
        /// </summary>
        public void Spend(float amount, Currencies currency) {
            _currencies[currency] -= amount;
            OnCurrencyUpdated.Invoke(_currencies[currency], amount, currency);
        }

        /// <summary>
        /// Note: it doesnt write to save data, if you need use <see cref="SpendAndSave(Cost)"/>
        /// </summary>
        public void Spend(Cost cost) {
            Spend(cost.Value, cost.Type);
        }

        public void SpendAndSave(float amount, Currencies currency) {
            _currencies[currency] -= amount;
            OnCurrencyUpdated.Invoke(_currencies[currency], amount, currency);
            _save.Data.Currencies[currency] = _currencies[currency];
        }

        public void SpendAndSave(Cost cost) {
            SpendAndSave(cost.Value, cost.Type);
        }

        public bool IsEnough(float forAmount, Currencies currency) {
            return _currencies[currency] >= forAmount;
        }
		
		public bool IsEnough(Cost cost) {
            return _currencies[cost.Type] >= cost.Value;
        }

        /// <param name="withSave">also write transaction to save data so action will be saved</param>
        public bool TrySpend(float amount, Currencies currency, bool withSave = true) {
            if (_currencies[currency] >= amount) {
                _currencies[currency] -= amount;
                OnCurrencyUpdated.Invoke(_currencies[currency], amount, currency);
                if (withSave) {
                    _save.Data.Currencies[currency] = _currencies[currency];
                }
                return true;
            }
            else {
                OnNotEnoughCurrency.Invoke(currency);
                return false;
            }
        }

        /// <param name="withSave">also write transaction to save data so action will be saved</param>
        public bool TrySpend(Cost cost, bool withSave = true) {
            return TrySpend(cost.Value, cost.Type, withSave);
        }

#if UNITY_EDITOR
        /// <summary>
        /// USE IT ONLY FOR TESTS IN EDITOR
        /// </summary>
        public int addOrRemoveCashAmountEDITOR = 500;
        public Currencies editorCurrency = Currencies.Cash;

        [Button("Get EDITOR cash")]
        private void AddEDITOR() {
            AddAndSave(addOrRemoveCashAmountEDITOR, editorCurrency);
        }

        [Button("Spend EDITOR cash")]
        private void SpendEDITORClamped() {
            SpendAndSave(addOrRemoveCashAmountEDITOR, editorCurrency);
        }

        [Button("Delete cash data")]
        private void DeleteEDITORCashData() {
            _currencies = new SaveData().Currencies;
            
            if (_allCurrencies != null) {
                foreach (var currency in _allCurrencies) {
                    OnCurrencyUpdated.Invoke(_currencies[currency], 0f, currency);
                }
            }
        }

        [Button("Log local manager currency values")]
        private void LogValues() {
            print(Newtonsoft.Json.JsonConvert.SerializeObject(_currencies));
        }
#endif
    }
}