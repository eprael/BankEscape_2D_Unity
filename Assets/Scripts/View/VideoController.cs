using UnityEngine;
using UnityEngine.Video;

public class VideoController : MonoBehaviour
{
    public VideoPlayer videoPlayer;

    /// <summary>
    /// Play a video by filename (should be in StreamingAssets).
    /// </summary>
    public void PlayVideo(string filename)
    {
        // Build the full path to the video in StreamingAssets
        string path = System.IO.Path.Combine(Application.streamingAssetsPath, filename);
        videoPlayer.Stop();
        videoPlayer.url = path;
        videoPlayer.gameObject.SetActive(true);

        videoPlayer.prepareCompleted -= OnPrepared;
        videoPlayer.prepareCompleted += OnPrepared;
        videoPlayer.Prepare();
    }

    private void OnPrepared(VideoPlayer vp)
    {
        vp.prepareCompleted -= OnPrepared;
        vp.Play();
    }

    public void StopVideo()
    {
        videoPlayer.Stop();
        videoPlayer.gameObject.SetActive(false);
        videoPlayer.prepareCompleted -= OnPrepared;
    }
}