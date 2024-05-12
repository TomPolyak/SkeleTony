using System.Collections;
using System.Collections.Generic;
using System.Xml.Resolvers;
using Unity.VisualScripting;
using UnityEngine;

public class ShotgunAnimationHandler : MonoBehaviour
{
    [SerializeField] private ParticleSystem casingEmitter;
    [SerializeField] private Animator gunAnimator;

    [SerializeField] private int maxShellCap;

    private int shells;
    private bool reloadComplete = true;

    public void reloadAndEmitCasings()
    {
        int shellsToEject = maxShellCap - shells;
        casingEmitter.Emit(shellsToEject);

        shells = maxShellCap;
        reloadComplete = true;
    }

    void Start()
    {
        gunAnimator = GetComponent<Animator>();
        shells = maxShellCap;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && (shells > 0)) {
            switch (reloadComplete && !gunAnimator.GetCurrentAnimatorStateInfo(0).IsName("gun_reload")) {
                case true:
                    shells--;
                    gunAnimator.SetTrigger("fire");
                    break;
                default:
                    break;
            }
        }

        if (((Input.GetKeyDown(KeyCode.R) && (shells != maxShellCap)) || (shells == 0)) && reloadComplete) 
        {
            gunAnimator.SetTrigger("reload");

            reloadComplete = false;
        }
    }
}
