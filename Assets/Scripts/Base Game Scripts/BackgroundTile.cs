using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundTile : MonoBehaviour
{
    public int hitPoints;
    private SpriteRenderer sprite;
    private GoalManager goalManager;

    private void Start()
    {
        goalManager = FindObjectOfType<GoalManager>();
        sprite = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        if (hitPoints <= 0)
        {
            if (goalManager != null)
            {
                goalManager.CompareGoal(this.gameObject.tag);
                goalManager.UpdateGoals();
            }
            Destroy(this.gameObject);
        }
    }

    public void TakeDamage(int damage)
    {
        hitPoints -= damage;
        MakeLighter();
    }

    void MakeLighter()
    {
        // текущий цвет спрайта
        Color color = sprite.color;
        // получить текущее значение прозрачности спрайта и уменьшить его наполовину
        float newAlpha = color.a * .5f;
        sprite.color = new Color(color.r, color.g, color.b, newAlpha);
    }
}
