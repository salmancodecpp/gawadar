using UnityEngine;
using System.Collections;
//Syed Raahim Bukhari
//Adnan Anwar Ali
// code written by Muhammad Saad Jumani
// this is an edited comment

public class CameraControl : MonoBehaviour
{
    public float speed = 0.1F;

    public float perspectiveZoomSpeed = 0.5f;        // The rate of change of the field of view in perspective mode.
    public float orthoZoomSpeed = 0.5f;        // The rate of change of the orthographic size in orthographic mode.
    Vector2 touchDeltaPosition;
    void Update()
    {

        if (Input.touchCount == 1 && Input.GetTouch(0).phase == TouchPhase.Moved)
        {
            // Get movement of the finger since last frame
            touchDeltaPosition = Input.GetTouch(0).deltaPosition;

            // Move object across XY plane
            transform.Translate(-touchDeltaPosition.x * speed, 0.0f, -touchDeltaPosition.y * speed, Space.World);
        }
        else if (Input.touchCount == 0 && (touchDeltaPosition.x > 0.0f || touchDeltaPosition.y > 0.0f))
        {
            transform.Translate(-touchDeltaPosition.x * speed, 0.0f, -touchDeltaPosition.y * speed, Space.World);
            touchDeltaPosition.x = touchDeltaPosition.x * 0.8f;
            touchDeltaPosition.y = touchDeltaPosition.y * 0.85f;
        }

        if (Input.touchCount == 2)
        {
            Touch touchZero = Input.GetTouch(0);
            Touch touchOne = Input.GetTouch(1);

            Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
            Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

            float prevTouchDeltaMag = (touchZeroPrevPos - touchOnePrevPos).magnitude;
            float touchDeltaMag = (touchZero.position - touchOne.position).magnitude;

            // Find the difference in the distances between each frame.
            float deltaMagnitudeDiff = prevTouchDeltaMag - touchDeltaMag;

            transform.Translate(0.0f, deltaMagnitudeDiff * speed, 0.0f, Space.World);
        }

    }
}
