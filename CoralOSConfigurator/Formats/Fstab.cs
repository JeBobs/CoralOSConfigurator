using System;
using System.IO;

namespace CoralOSConfigurator.Formats
{
    public class Fstab
    {
        string FilePath;
        TableEntry[] Entries;

        public Fstab()
        {
            FilePath = "/etc/fstab";
        }
        public bool SetEntries(TableEntry[] entries)
        {
            if (entries.Length > 0)
            {
                Entries = entries;
                return true;
            }
            return false;
        }
    }

    public struct TableEntry
    {
        string DeviceID;
        string MountLocation;
        string FilesystemType;
        string[] MountOptions;
        bool Dump;
        ushort FsckOrder;

        // TODO: More sanity checking here
        public bool SetEntry
        (
            string deviceID, 
            string mountLocation, 
            string filesystemType, 
            string[] mountOptions, 
            bool dump, 
            ushort fsckOrder
        )
        {
            // DeviceID
            DeviceID = deviceID;

            // MountLocation
            if (mountLocation.Length > 4096)
            {
                return false;
            }
            MountLocation = mountLocation;

            // FilesystemType
            FilesystemType = filesystemType;

            // MountOptions
            MountOptions = mountOptions;

            // Dump
            Dump = dump;

            // FsckOrder
            FsckOrder = fsckOrder;
    
            return true;
        }

        public TableEntry GetEntry()
        {
            return this;
        }
    }

    public static class FstabAPI
    {
        public static bool LoadFstabFile(string Path, out Fstab outFstab)
        {
            outFstab = new Fstab();

            if (File.Exists(Path))
            {
                string[] FstabLines = File.ReadAllLines(Path);

                TableEntry[] entries;

                ParseEntries(FstabLines, out entries);

                outFstab.SetEntries(entries);

                return true;
            }
            // Replace this
            return false;
        }

        public static bool ParseEntries(string[] lines, out TableEntry[] tableEntries)
        {
            tableEntries = new TableEntry[0];
            if (lines.Length > 0) 
            {
                foreach (string line in lines)
                {
                    if (!line.StartsWith("#") || !string.IsNullOrWhiteSpace(line))
                    {
                        // Split parts of the individual line
                        string[] parts = line.Split();
                        if (parts.Length != 6)
                        {
                            // Log error here
                            continue; // to next line
                        }

                        // Since we are a valid entry,
                        // create our temporary table entry
                        TableEntry tableEntry = new TableEntry();

                        // Parse Fsck Order into ushort (uint16)
                        ushort FsckOrder;
                        if (!ushort.TryParse(parts[5], out FsckOrder))
                        {
                            // Log Error here
                            continue; // to next line
                        }

                        // Set parts into entry
                        tableEntry.SetEntry(parts[0], parts[1], parts[2], parts[3].Split(","), parts[4] != "0", FsckOrder);

                        // Set array element, there is probably a better way to do this.
                        Array.Resize(ref tableEntries, tableEntries.Length + 1);
                        tableEntries[tableEntries.Length - 1] = tableEntry;
                    }
                }
                return true;
            }
            return false;
        }
    }
}