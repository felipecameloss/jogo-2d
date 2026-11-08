using System.Collections.Generic;
using UnityEngine;

// Guarda as armas que o jogador tem.
public class WeaponHolder : MonoBehaviour
{
    public Weapon startingWeapon;

    readonly List<Weapon> weapons = new List<Weapon>();
    readonly Dictionary<Weapon, Weapon> instancesByPrefab = new Dictionary<Weapon, Weapon>();

    public IReadOnlyList<Weapon> Weapons => weapons;

    void Start()
    {
        if (startingWeapon) AddWeapon(startingWeapon);
    }

    public bool Has(Weapon prefab) => instancesByPrefab.ContainsKey(prefab);

    // Instância que o jogador tem desse prefab, ou null.
    public Weapon Get(Weapon prefab) => prefab && instancesByPrefab.TryGetValue(prefab, out var weapon) ? weapon : null;

    // Arma nova entra no nível 1; arma repetida sobe um nível. Devolve false se já estava no máximo.
    public bool Unlock(Weapon prefab)
    {
        if (!prefab) return false;
        var owned = Get(prefab);
        if (!owned) { AddWeapon(prefab); return true; }
        if (owned.IsMaxLevel) return false;
        owned.LevelUp();
        return true;
    }

    public Weapon AddWeapon(Weapon prefab)
    {
        if (instancesByPrefab.TryGetValue(prefab, out var existing)) return existing;

        var weapon = Instantiate(prefab, transform);
        weapon.transform.localPosition = Vector3.zero;
        weapon.Init(GetComponent<PlayerStats>());
        weapons.Add(weapon);
        instancesByPrefab[prefab] = weapon;
        return weapon;
    }
}
