using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Cinemachine;
using System;

public class GamePlayManager : Singleton<GamePlayManager>
{
    public CinemachineFreeLook _FreeLookCamera;
    public float _MilestoneSize;
    public float _RadiusIncreasePerMilestone;

    public float pScore;
    private float mMilestoneProgress;

    public TextMeshProUGUI _ScoreText;
    private string mCachedScoreText;

    /// <summary>
    /// Initializes mCachedScoreText and updates the UI
    /// Then if no CinemachineFreeLook camera is assigned, assign it.
    /// _FreeLookCamera SHOULD be assigned in the editor, this is just a fallback.
    /// </summary>
    private void Start()
    {
        mCachedScoreText = _ScoreText.text;
        UpdateScoreText();

        if (!_FreeLookCamera)
            _FreeLookCamera = FindObjectOfType<CinemachineFreeLook>();
    }

    /// <summary>
    /// Adds to the score (rounded to two significant digits)
    /// Then adds progress to mMilestoneProgress
    /// And updates the score with OnScoreUpdated();
    /// </summary>
    /// <param name="inScore"></param>
    public void AddToScore(float inScore)
    {
        pScore += inScore;
        pScore = Mathf.Round(pScore * 100.0f) / 100.0f;
        mMilestoneProgress += inScore;
        OnScoreUpdated();
    }

    /// <summary>
    /// Triggered every time the score is updated i.e. from AddToScore
    /// If a milestone has been reached, call UpdateCameraRig to increase the camera orbit radius
    /// </summary>
    private void OnScoreUpdated()
    {
        UpdateScoreText();

        if(mMilestoneProgress > _MilestoneSize)
            UpdateCameraRig();
    }

    /// <summary>
    /// Updates the score text with a formatted string including the new score
    /// </summary>
    private void UpdateScoreText()
    {
        _ScoreText.text = string.Format(mCachedScoreText, pScore);
    }

    /// <summary>
    /// Calculates how large to set the radii of each orbit in _FreeLookCamera
    /// Then starts the IncreaseCameraRigRadius coroutine
    /// And resets the milestone progress
    /// </summary>
    private void UpdateCameraRig()
    {
        float radiusIncrease = Mathf.Floor(pScore / _MilestoneSize) * _RadiusIncreasePerMilestone;

        StartCoroutine(IncreaseCameraRigRadius(radiusIncrease, 1f));

        mMilestoneProgress = 0;
    }

    /// <summary>
    /// Smoothly increases the radius of every orbit of the CinemachineFreeLook component _FreeLookCamera
    /// This is so as your Katamari increases in size, the camera adjusts so the Katamari won't obscure your view
    /// </summary>
    /// <param name="radiusIncrease"></param>
    /// <param name="duration"></param>
    /// <returns></returns>
    private IEnumerator IncreaseCameraRigRadius(float radiusIncrease, float duration)
    {
        List<float> initialRadii = new List<float>();

        foreach (CinemachineFreeLook.Orbit orbit in _FreeLookCamera.m_Orbits)
            initialRadii.Add(orbit.m_Radius);

        List<float> targetRadii = new List<float>();
        foreach (float f in initialRadii)
            targetRadii.Add(f + radiusIncrease);

        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;

            for (int i = 0; i < _FreeLookCamera.m_Orbits.Length; i++)
                _FreeLookCamera.m_Orbits[i].m_Radius = Mathf.Lerp(initialRadii[i], targetRadii[i], elapsedTime / duration);

            yield return null;
        }

        for (int i = 0; i < _FreeLookCamera.m_Orbits.Length; i++)
            _FreeLookCamera.m_Orbits[i].m_Radius = targetRadii[i];
    }
}
