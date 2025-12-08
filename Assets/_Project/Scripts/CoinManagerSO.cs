using System;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "CoinManagerSO", menuName = "ARG/CoinManagerSO")]
public class CoinManagerSO : ScriptableObject
{
    [SerializeField,ReadOnly] private int currentCoins;

    private bool saveCoinsOnValueChange = true;
    
    public int CurrentCoins
    {
        get => currentCoins;
        private set
        {
            int difference = value - currentCoins;
            currentCoins = value;
            OnCoinsUpdated?.Invoke(difference, currentCoins);

            if (saveCoinsOnValueChange)
            {
                PlayerPrefs.SetInt(StringID.TotalCoins,currentCoins);
                PlayerPrefs.Save();
            }
        }
    }

    private readonly int defaultValue = 0;

    public event Action<int, int> OnCoinsUpdated;
    public event Action<int> OnCoinsAdded;
    public event Action<int> OnCoinsDeducted;
    public event Action<int> OnCoinDeductFailed;

    public void Init()
    {
        CurrentCoins = PlayerPrefs.GetInt(StringID.TotalCoins,defaultValue);
    }
    
    public void AddCoins(int incrementValue)
    {
        OnCoinsAdded?.Invoke(incrementValue);
        CurrentCoins += incrementValue;
    }

    public bool TryDeductCoins(int deductValue)
    {
        if (CurrentCoins <= deductValue)
        {
            OnCoinDeductFailed?.Invoke(deductValue - CurrentCoins);
            return false;
        }
        
        OnCoinsDeducted?.Invoke(deductValue);
        CurrentCoins -= deductValue;
        return true;
    }

    public void ToggleSaveCoins(bool value)
    {
        saveCoinsOnValueChange = value;
        if(value) SaveTotalCoins();
    }
    
    public void SaveTotalCoins()
    {
        PlayerPrefs.SetInt(StringID.TotalCoins,currentCoins);
        PlayerPrefs.Save();
    }

    public void SetCoins(int value)
    {
        CurrentCoins = value;
    }
}