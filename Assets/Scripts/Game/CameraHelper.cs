using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

/// <summary>
/// Helper script used for managing camera letterbox/pillarbox.
/// </summary>
[ExecuteInEditMode]
public class CameraHelper : MonoBehaviour
{
    /// <summary>
    /// Target camera size in units.
    /// </summary>
    [Header("Global")]
    public Vector2 targetResolution = new Vector2(4.0f, 4.0f);

    /// <summary>
    /// How fast should the zooming work?
    /// </summary>
    public float zoomSpeed = 0.1f;

    /// <summary>
    /// Set a GameObject to follow.
    /// </summary>
    public GameObject followTarget;
    
    /// <summary>
    /// Follow the target GameObject?
    /// </summary>
    public bool doFollowTarget = true;
    
    /// <summary>
    /// The managed camera component.
    /// </summary>
    private Camera mCamera;

    /// <summary>
    /// Current resolution we are working with.
    /// </summary>
    private Vector2 mResolution;
    
    /// <summary>
    /// Current target we are working with.
    /// </summary>
    private Vector2 mTarget;

    

    
    /// <summary>
    /// Called before the first frame update.
    /// </summary>
    void Start()
    { mCamera = GetComponent<Camera>(); }

    /// <summary>
    /// Update called once per frame.
    /// </summary>
    void Update()
    {
        // Fit the camera to the target resolution, if necessary.
        FitTargetResolution(targetResolution);

        //No idea why but without this check I constantly get an error in editor about the instance possibly being null
        //This isnt the case with scripts for some reason
        if (GameManager.Instance != null){

            var livingPlayerCount = GameManager.Instance.LivingPlayers().Count;
            var currentPosition = transform.position;
            var targetPosition = new Vector3 { x = 0f,y = 0f, z = 0f};
            
            
            
            // Follow the target, if enabled.
            if (doFollowTarget){
                
                //Calculate midpoint
                foreach (GameObject player in GameManager.Instance.LivingPlayers())
                {
                    targetPosition = targetPosition + player.transform.position;
                }
                targetPosition = (targetPosition / livingPlayerCount) - transform.forward;

                if(livingPlayerCount == 2){
                    float distance = Vector3.Distance(GameManager.Instance.LivingPlayers()[0].transform.position, GameManager.Instance.LivingPlayers()[1].transform.position);
                    if (distance > 6){
                        targetResolution = new Vector2(0.1f*(distance+4)*6.0f,0.1f*(distance+4)*6.0f);
                    }else{
                        targetResolution = new Vector2(6.0f,6.0f);
                    }
                }else{
                    targetResolution = new Vector2(6.0f,6.0f);
                }



                //Edited followtarget to use vector 3 instead of gameobject
                //Move to the midpoint
                FollowTarget(targetPosition); 

            }
        }
    }

    public void FitTargetResolution(Vector2 target)
    {
        // Update is only needed when the resolution is changed.
        Vector2 currentResolution = new Vector2((float)Screen.width, (float)Screen.height);
        if (mTarget.Equals(target) && mResolution.Equals(currentResolution))
        { return; }

        // Set the extent of size we want to use.
        var cameraSize = Math.Max(target.x, target.y);
        mCamera.orthographicSize = cameraSize;
        
        // Calculate the current aspect ratio of the screen and the requested target.
        var currentAspectRatio = (float)Screen.width / Screen.height;
        var targetAspectRatio = target.x / target.y;
        // How much of a letterbox do we need?
        var letterboxRatio = currentAspectRatio / targetAspectRatio;

        // Prepare letterbox-ed rectangle for the camera.
        var cameraRect = new Rect();
        if (letterboxRatio >= 1.0f)
        { // The screen is too wide -> Vertical letterbox.
            var letterboxWidth = 1.0f / letterboxRatio;
            cameraRect.x = (1.0f - letterboxWidth) / 2.0f;
            cameraRect.y = 0.0f;
            cameraRect.width = letterboxWidth;
            cameraRect.height = 1.0f;
        }
        else
        { // The screen is too high -> Horizontal letterbox.
            var letterboxHeight = letterboxRatio;
            cameraRect.x = 0.0f;
            cameraRect.y = (1.0f - letterboxHeight) / 2.0f;
            cameraRect.width = 1.0f;
            cameraRect.height = letterboxHeight;
        }

        // Update the camera to include our new letterbox.
        mCamera.rect = cameraRect;
        mResolution = currentResolution;
        mTarget = target;
    }

    public void FollowTarget(Vector3 targetPosition)
    {
        
        // Move the camera to be above the targetPosition.
        var currentPosition = transform.position;
        transform.position = new Vector3{
            x = targetPosition.x, 
            y = currentPosition.y, 
            z = targetPosition.z
        };
    }

    /// <summary>
    /// Zoom the current view by given magnitude.
    /// </summary>
    /// <param name="magnitude">By how much should the view change.</param>
    public void ZoomView(float magnitude)
    {
        // Clamp the values to be at least 1.0f.
        targetResolution.x = Math.Max(targetResolution.x + magnitude * zoomSpeed, 1.0f);
        targetResolution.y = Math.Max(targetResolution.y + magnitude * zoomSpeed, 1.0f);
    }
}
