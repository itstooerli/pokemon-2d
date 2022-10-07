using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HPBar : MonoBehaviour
{
    [SerializeField] GameObject health;
    
    // Start is called before the first frame update
    void Start()
    {

    }

    public void SetHP(float hpNormalized)
    {
        health.transform.localScale = new Vector3(hpNormalized, 1f);
    }

    public IEnumerator SetHPSmooth(float newHp)
    {
        float currHp = health.transform.localScale.x;
        
        // CUSTOM: Allow smooth even when adding HP
        if (currHp > newHp)
        {
            float changeAmt = currHp - newHp;
            while (currHp - newHp > Mathf.Epsilon)
            {
                currHp -= changeAmt * Time.deltaTime;
                health.transform.localScale = new Vector3(currHp, 1f);
                yield return null;
            }
        }
        else
        {
            float changeAmt = newHp - currHp;
            while (newHp - currHp > Mathf.Epsilon)
            {
                currHp += changeAmt * Time.deltaTime;
                health.transform.localScale = new Vector3(currHp, 1f);
                yield return null;
            }
        }

        health.transform.localScale = new Vector3(newHp, 1f);
    }
}
