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
