using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelUpPanel : MonoBehaviour
{
    public Text headerText;
    public Button[] optionButtons;
    public Text[] optionTitles;
    public Text[] optionDescriptions;

    Action<int> onPicked;
    int optionCount;

    public void Show(IList<UpgradeOption> options, int level, Action<int> picked)
    {
        gameObject.SetActive(true);
        onPicked = picked;
        optionCount = Mathf.Min(options.Count, optionButtons.Length);
        if (headerText) headerText.text = $"NÍVEL {level}!";

        for (int i = 0; i < optionButtons.Length; i++)
        {
            bool used = i < optionCount;
            optionButtons[i].gameObject.SetActive(used);
            if (!used) continue;

            optionTitles[i].text = $"[{i + 1}] {options[i].Title}";
            optionDescriptions[i].text = options[i].Description;
            int index = i;
            optionButtons[i].onClick.RemoveAllListeners();
            optionButtons[i].onClick.AddListener(() => Pick(index));
        }
    }

    void Update()
    {
        int k = GameInput.OptionPressed();
        if (k >= 0 && k < optionCount) Pick(k);
    }

    void Pick(int index)
    {
        if (onPicked == null) return; // evita escolher duas vezes
        var callback = onPicked;
        onPicked = null;
        gameObject.SetActive(false);
        AudioManager.Play(Sfx.Select);
        callback(index);
    }
}
