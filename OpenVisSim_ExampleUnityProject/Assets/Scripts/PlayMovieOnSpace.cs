using UnityEngine;
using System.Collections;

public class PlayMovieOnSpace : MonoBehaviour
{
    void Update()
    {
        // MovieTexture doesn't work on webgl or phones (?)
#if UNITY_EDITOR
        if (Input.GetButtonDown("Jump"))
        {

            Renderer r = GetComponent<Renderer>();
            MovieTexture movie = (MovieTexture)r.material.mainTexture;

            if (movie.isPlaying)
            {
                movie.Pause();
            }
            else {
                movie.Play();
            }
        }
#endif
    }
}
