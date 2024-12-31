using UnityEngine;
using System;
using System.IO;

public class ConsoleLogExporter : MonoBehaviour
{
    [SerializeField]
    private string fileName = "game_log.txt";
    private string logFilePath;
    
    private void Awake()
    {
        // Use persistent data path instead of Assets folder
        logFilePath = Path.Combine(Application.persistentDataPath, fileName);
        Debug.Log($"Log file will be saved to: {logFilePath}");
        
        // Create directory if it doesn't exist
        string directory = Path.GetDirectoryName(logFilePath);
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        // Clear the file at start
        try
        {
            File.WriteAllText(logFilePath, $"=== New Session Started: {DateTime.Now} ===\n");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to initialize log file: {e.Message}");
        }
    }

    private void OnEnable()
    {
        Application.logMessageReceived += HandleLog;
        Debug.Log("ConsoleLogExporter enabled and listening for logs");
    }

    private void OnDisable()
    {
        Application.logMessageReceived -= HandleLog;
    }

    void HandleLog(string logString, string stackTrace, LogType type)
    {
        // Skip logging our own initialization message to avoid recursion
        if (logString.Contains("ConsoleLogExporter enabled"))
            return;

        try
        {
            string formattedMessage = FormatLogMessage(logString, stackTrace, type);
            File.AppendAllText(logFilePath, formattedMessage);
        }
        catch (Exception e)
        {
            // Use Unity's internal debug to avoid infinite recursion
            Debug.unityLogger.LogError("ConsoleLogExporter", $"Failed to write to log file: {e.Message}");
        }
    }

    private string FormatLogMessage(string logString, string stackTrace, LogType type)
    {
        string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
        string logLevel = type.ToString().ToUpper();
        string message = $"[{timestamp}] {logLevel}: {logString}\n";
        
        // Add stack trace for errors and exceptions
        if (type == LogType.Error || type == LogType.Exception)
        {
            message += $"Stack Trace:\n{stackTrace}\n";
        }
        
        return message;
    }

    // Optional: Method to get the log file location
    public string GetLogFilePath()
    {
        return logFilePath;
    }
}