using System.Collections;
using UnityEngine;
using TMPro;

public class QTE_line : MonoBehaviour
{
    public float Speed = 3f;
    public Transform uppoint, downpoint;

    public int baseScore = 10;
    private float UpY, DownY, targetY;

    private string currentZone = "gray";
    private string lastScoredZone = "";
    private bool canClick = true;

    private int redCount = 0;
    private int blueCount = 0;
    private int grayCount = 0;

    private bool isCounting = false;
    private bool isMoving = false;
    private float countDuration = 10f;
    private float timer;

    public TMP_Text countdownText;
    public TMP_Text scoreText;
    public TMP_Text startcount;

    void Start()
    {
        UpY = uppoint.position.y;
        DownY = downpoint.position.y;
        targetY = DownY;

        Destroy(uppoint.gameObject);
        Destroy(downpoint.gameObject);

        StartCoroutine(PrepareAndStartQTE());
    }

    void Update()
    {
        float step = Speed * Time.deltaTime;

        if (isMoving)
        {
            Vector3 targetPosition = new Vector3(transform.position.x, targetY, transform.position.z);
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, step);
        }

        if (Mathf.Abs(transform.position.y - targetY) < 0.05f)
        {
            targetY = targetY == UpY ? DownY : UpY;
        }

        if (isCounting)
        {
            timer -= Time.deltaTime;
            countdownText.text = $"SEC：{Mathf.CeilToInt(timer)} ";

            if (Input.GetKeyDown(KeyCode.Space) && canClick)
            {
                switch (currentZone)
                {
                    case "red":
                        redCount++;
                        scoreText.text = "Area：RED (*2)";
                        break;
                    case "blue":
                        blueCount++;
                        scoreText.text = "Area：BLUE (*1.5)";
                        break;
                    case "gray":
                        grayCount++;
                        scoreText.text = "Area：GRAY (*1)";
                        break;
                }

                lastScoredZone = currentZone;
                canClick = false;
            }

            if (timer <= 0f)
            {
                isCounting = false;
                isMoving = false;
                CalculateAndDisplayResult();
            }
        }
    }

    private IEnumerator PrepareAndStartQTE()
    {
        startcount.text = "Wait for start";
        scoreText.text = "";
        countdownText.text = "";
        yield return new WaitForSeconds(1f);

        for (int i = 3; i > 0; i--)
        {
            startcount.text = i.ToString();
            yield return new WaitForSeconds(1f);
        }

        startcount.text = "START！";
        yield return new WaitForSeconds(1f);

        isCounting = true;
        isMoving = true;
        timer = countDuration;
        countdownText.text = $"SEC：{Mathf.CeilToInt(timer)} ";
        startcount.text = "";
    }

    private void CalculateAndDisplayResult()
    {
        float multiplier = 1f;
        string zone = "gray";

        if (redCount >= blueCount && redCount >= grayCount)
        {
            multiplier = 2f;
            zone = "red";
        }
        else if (blueCount >= redCount && blueCount >= grayCount)
        {
            multiplier = 1.5f;
            zone = "blue";
        }

        int finalScore = Mathf.RoundToInt(baseScore * multiplier);

        startcount.text = "END！";
        scoreText.text = $"Most：{zone}\n（score* {multiplier} !）\n";
        Debug.Log($"最多按到：{zone} → 最終得分：{finalScore}");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("red") || other.CompareTag("blue") || other.CompareTag("gray"))
        {
            string newZone = other.tag;

            if (newZone != currentZone)
            {
                canClick = true;
            }

            currentZone = newZone;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        OnTriggerEnter(other);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("red") || other.CompareTag("blue") || other.CompareTag("gray"))
        {
            currentZone = "gray";
        }
    }
}
