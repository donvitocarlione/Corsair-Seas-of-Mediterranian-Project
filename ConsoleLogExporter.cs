using UnityEngine;
using System.IO;
using System;

public class ConsoleLogExporter : MonoBehaviour
{
    private string logFilePath;
    private StreamWriter writer;
    public bool exportOnStart = true;
    public bool exportOnQuit = true;


    void Start()
    {
        // Determine the log file path (you can customize this)
        string fileName = "unity_console_log_" + DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + ".txt";
        logFilePath = Path.Combine(Application.persistentDataPath, fileName);

        if (exportOnStart)
        {
             StartExport();
        }
    }

    void OnEnable()
    {
        // Hook into the log callback
        Application.logMessageReceived += LogCallback;
    }
    void OnDisable()
    {
       // Unhook into the log callback
        Application.logMessageReceived -= LogCallback;
    }
    void OnApplicationQuit()
    {
        if (exportOnQuit)
        {
            StopExport();
        }
    }

    void StartExport()
    {
        try
        {
            // Create the file or overwrite it
            writer = new StreamWriter(logFilePath, false); // "false" means overwrite
            Debug.Log($"Log export started. File: {logFilePath}");
        }
        catch (Exception ex)
        {
           Debug.LogError($"Error creating log file: {ex.Message}");
        }
    }

    void StopExport()
    {
        if (writer != null)
        {
            writer.Close(); // important
            writer = null;
            Debug.Log($"Log export stopped. File: {logFilePath}");
        }
    }

    private void LogCallback(string logString, string stackTrace, LogType type)
    {
        if(writer != null)
        {
             // Determine a prefix for the message
            string typeString = type.ToString().ToUpper();
            string outputString = $"[{typeString}] {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")} - {logString}";

            // Add stack trace only for error logs
            if (type == LogType.Error || type == LogType.Exception)
            {
                outputString += $"\nStackTrace: {stackTrace}";
            }

            // Write to file
            writer.WriteLine(outputString);
            writer.Flush();
        }
    }
}