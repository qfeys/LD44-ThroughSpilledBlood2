using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

class Ability
{

    float cost;
    public float cooldown;

    public delegate void AbilityAction(Vector2 pos, Vector2 trgt);
    AbilityAction action;

    public Ability(AbilityAction action)
    {
        this.action = action;
    }

    internal void Fire(Vector2 pos, Vector2 trgt)
    {
        action(pos, trgt);
    }
}
