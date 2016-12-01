using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CameraFocus : MonoBehaviour {

    public List<GameObject> playerObjects;
    public GameObject localPlayerObject;
    public GameObject[] arrows;
    public GameObject[] limitColiders;
    public float arrowOffset;
    public float minSize;
    public float maxSize;
    //a float number that represents the percentage of the camera that the players may stay in before it resizes
    public float minThreshold; 
    public float maxThreshold;
    public float horizontalLimit;
    public float verticalLimit;
    public float lerpPercentage;
    public float lerpLimit;
    public float spectateMinSize;
    public float spectateMaxSize;

    private Camera _camera;
    private Vector3 _targetPosition;
    private float _targetScale;
    public Vector2 _arrowSize;
    public LayerMask __layerMask;
    private bool _spectating = false;

    void Awake()
    {
        //Debug.Log("Camera: começou a procurar pelos player");
        playerObjects = new List<GameObject>();
        _camera = GetComponent<Camera>();
        //__layerMask = 1 << LayerMask.NameToLayer("CameraLimits");
    }
	
	void Start () {
        
        
	}    
		
	void FixedUpdate () {
        //first we scale it properly
        SetTargetScale();

        //next we position it properly
        SetTargetPosition();

        LerpToTarget();

        UpdateLimitColliders();

        UpdateArrows();      
	}

    public Vector2 GetFocusPoint()
    {
        int __disabledCount = 0;
        int __localPlayerWeight = 0;
        float __playerWeight = 1;
        Vector2 __focus = Vector2.zero;

        /**
        if (playerObjects.Count > 1)
        {
            for (int i = 0; i < playerObjects.Count; i++)
            {
                if (playerObjects[i] != null)
                {
                    if (playerObjects[i].activeSelf)
                    {
                        __focus += (Vector2)playerObjects[i].transform.position;
                    }
                }
                else
                {
                    __disabledCount++;
                }
            }
        }
        /**

        if(localPlayerObject != null)
        {
            if (localPlayerObject.activeSelf)
            {
                __localPlayerWeight = 10;
                __playerWeight = 1f / __localPlayerWeight;
                for (int i = 0; i < __localPlayerWeight; i++)
                {
                    __focus += (Vector2)localPlayerObject.transform.position;
                }
                _spectating = false;
            }
            else
            {
                _spectating = true;
            }
        }
        /**/

        if (localPlayerObject != null)
        {
            if (localPlayerObject.activeSelf)
            {
                __localPlayerWeight = 1;
                //__playerWeight = 1f / __localPlayerWeight;
                for (int i = 0; i < __localPlayerWeight; i++)
                {
                    __focus += (Vector2)localPlayerObject.transform.position;
                }
                _spectating = false;
            }
            else
            {
                _spectating = true;
            }
        }

        //return (__focus*__playerWeight) / ((playerObjects.Count - __disabledCount) < 1 ? 1 : (playerObjects.Count - __disabledCount));
        return __focus;
        /**/
    }

    public float GetScale()
    {
        float __camSize = 0;
        Vector2 __focus = GetFocusPoint();
        Vector2 __higherDist = Vector2.zero;
        Vector2 __tempDist;

        //check the higher distances from the focus point in X and Y directions
        for (int i = 0; i < playerObjects.Count; i++)
        {
            if (playerObjects[i] != null)
            {
                if (playerObjects[i].activeSelf)
                {
                    __tempDist = (Vector2)playerObjects[i].transform.position - __focus;
                    if (Mathf.Abs(__tempDist.x) > __higherDist.x) __higherDist.x = Mathf.Abs(__tempDist.x);
                    if (Mathf.Abs(__tempDist.y) > __higherDist.y) __higherDist.y = Mathf.Abs(__tempDist.y);
                }
            }
        }

        //now we change the size accordingly to those distances, depending on which axis is more significant
        
        if (__higherDist.y / (_camera.orthographicSize) > __higherDist.x / (_camera.orthographicSize * _camera.aspect))
        {
            __camSize = __higherDist.y / maxThreshold;
            //Debug.Log("CamSize y: " + __higherDist.y +" / " + maxThreshold);
        }
        else
        {
            __camSize = (__higherDist.x / maxThreshold) / _camera.aspect;
            //Debug.Log("CamSize x: ( " + __higherDist.x + " / " + maxThreshold + " ) / " + _camera.aspect);
        }
        if (!_spectating)
        {
            if (__camSize < minSize) __camSize = minSize;
            else if (__camSize > maxSize) __camSize = maxSize;
        }
        else
        {
            if (__camSize < spectateMinSize) __camSize = spectateMinSize;
            else if (__camSize > spectateMaxSize) __camSize = spectateMaxSize;
        }
        
        
        return __camSize;
    }

    public void AddPlayerObject(GameObject p_GO)
    {
        playerObjects.Add(p_GO);
    }

    public void AddLocalPlayerObject(GameObject p_GO)
    {
        localPlayerObject = p_GO;
        for (int i = 0; i < playerObjects.Count; i++)
        {
            if (playerObjects[i] == p_GO) playerObjects.Remove(p_GO);
        }
    }

    public void SetTargetScale()
    {
        //_camera.orthographicSize = GetScale();
        _targetScale = GetScale();
    }

    public void SetTargetPosition()
    {
        Vector3 __position = (Vector3)GetFocusPoint() + new Vector3(0, 0, -10);

       
        if (__position.x + (_camera.orthographicSize * _camera.aspect) > horizontalLimit)
        {
            __position.x = horizontalLimit - (_camera.orthographicSize * _camera.aspect);
        }
        else if ((__position.x - (_camera.orthographicSize * _camera.aspect) < -horizontalLimit))
        {
            __position.x = -horizontalLimit + (_camera.orthographicSize * _camera.aspect);
        }

        if (__position.y + (_camera.orthographicSize) > verticalLimit)
        {
            __position.y = verticalLimit - (_camera.orthographicSize);
        }
        else if ((__position.y - (_camera.orthographicSize) < -verticalLimit))
        {
            __position.y = -verticalLimit + (_camera.orthographicSize);
        }

        //transform.position = __position;
        _targetPosition = __position;
    }

    public void LerpToTarget()
    {
        Vector3 __tempPosition = Vector3.Lerp(transform.position, _targetPosition, lerpPercentage);
        if(Vector3.Distance(__tempPosition, _targetPosition) > lerpLimit)
        {
            transform.position = __tempPosition;
        }
        else
        {
            //Debug.Log(_targetPosition == null);
            //if (_targetPosition != null) Debug.Log(_targetPosition);
            transform.position = _targetPosition;
        }

        float __tempScale = _camera.orthographicSize + ((_targetScale - _camera.orthographicSize) * lerpPercentage);
        if (_targetScale - __tempScale > lerpLimit)
        {
            _camera.orthographicSize = __tempScale;
        }
        else
        {
            _camera.orthographicSize = _targetScale;
        }        
    }

    public void UpdateArrows()
    {
        for (int i = 0; i < playerObjects.Count; i++)
        {
            if (_spectating)
            {
                if (playerObjects[i] != null)
                {
                    if (!playerObjects[i].GetComponent<PlayerManager>().bodySprite.isVisible && playerObjects[i].activeSelf)
                    {
                        Vector2 __direction = playerObjects[i].transform.position - transform.position;
                        RaycastHit2D __hit = Physics2D.Raycast(transform.position, __direction, 100, __layerMask.value);
                        float __angle = Mathf.Atan2(__direction.y, __direction.x);
                        if (__angle < 0) __angle += Mathf.PI * 2;

                        arrows[i].SetActive(true);
                        arrows[i].transform.eulerAngles = new Vector3(0, 0, Mathf.Rad2Deg * __angle);
                        arrows[i].transform.position = __hit.point;
                        arrows[i].GetComponent<SpriteRenderer>().color = playerObjects[i].GetComponent<PlayerManager>().bodySprite.GetComponent<SpriteRenderer>().color;
                    }
                    else
                    {
                        arrows[i].SetActive(false);
                    }
                }
            }
            else
            {
                arrows[i].SetActive(false);
            }       
        }
    }

    public void UpdateLimitColliders()
    {
        Vector2 __camPosition = transform.position;
        Vector2 __lowerPosition = Camera.main.ScreenToWorldPoint(Vector2.zero);
        Vector2 __upperPosition = Camera.main.ScreenToWorldPoint(new Vector2(Screen.width, Screen.height));

        //West Collider
        limitColiders[0].transform.position = new Vector2(__lowerPosition.x + arrowOffset, __camPosition.y);
        //North Collider
        limitColiders[1].transform.position = new Vector2(__camPosition.x, __upperPosition.y - arrowOffset);
        //East Collider
        limitColiders[2].transform.position = new Vector2(__upperPosition.x - arrowOffset, __camPosition.y);
        //South Collider
        limitColiders[3].transform.position = new Vector2(__camPosition.x, __lowerPosition.y + arrowOffset);
    }
}