using System.Text.Json;
using System.Xml.Serialization;

namespace LmuStatsViewer.Domain;

public class DrivingSession
{
    public string SessionFile { get; set; } = string.Empty;
    
    public string TrackCourse { get; set; } = string.Empty;
    public string TrackEvent { get; set; } = string.Empty;
    public string TrackVenue { get; set; } = string.Empty;
    public double TrackLengthInMeters { get; set; }
    
    public double BestLapTime { get; set; }
    public string CarClass { get; set; } = string.Empty;
    public string CarType { get; set; } = string.Empty;
    
    public SessionType SessionType { get; set; } = SessionType.Unknown;
    public double CompletedLaps { get; set; }

    public double DrivenDistanceInMeters => TrackLengthInMeters * CompletedLaps;
    
    private static XmlSerializer serializer = new XmlSerializer(typeof(RFactorXml));

    public static DrivingSession MakeFromFile(string filePath)
    {
        DrivingSession session = new DrivingSession();

        if (File.Exists(filePath))
        {
            try
            {
                var plainXmlForCleanup = File.ReadAllText(filePath);
                plainXmlForCleanup = plainXmlForCleanup.Replace("\uFFFD", "");
        
                using var reader = new StringReader(plainXmlForCleanup);
                var result = (RFactorXml)serializer.Deserialize(reader);
                Console.WriteLine(Path.GetFileName(filePath));

                if (result == null) return session;

                session.SessionFile = Path.GetFileName(filePath);

                session.TrackCourse = result.RaceResults.TrackCourse;
                session.TrackEvent = result.RaceResults.TrackEvent;
                session.TrackVenue = result.RaceResults.TrackVenue;
                session.TrackLengthInMeters = result.RaceResults.TrackLength;

                XmlDrivingSession? xmlSession = null;
                XmlDrivingSession[] allXmlSessions =
                [
                    result.RaceResults.Practice1, result.RaceResults.Practice2, result.RaceResults.Practice3,
                    result.RaceResults.Practice4, result.RaceResults.Qualify, result.RaceResults.Race
                ];

                for (int i = 0; i < allXmlSessions.Length; i++)
                {
                    if (allXmlSessions[i] != null)
                    {
                        xmlSession = allXmlSessions[i];
                        if (i <= 3) session.SessionType = SessionType.Practice;
                        else if (i == 4) session.SessionType = SessionType.Qualify;
                        else session.SessionType = SessionType.Race;
                    }
                }

                if (xmlSession != null)
                {
                    Driver? player = xmlSession.Drivers.Find(x => x.Name == Settings.PlayerName);
                    if (player != null)
                    {
                        session.CompletedLaps = EstimateCompletedLaps(player);
                        session.BestLapTime = player.BestLapTime;
                        session.CarClass = player.CarClass;
                        session.CarType = player.CarType;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error reading file: " + Path.GetFileName(filePath));
                Console.WriteLine(e.Message);
            }
            // TODO (JB) You are here.
        }

        return session;
    }

    private static double EstimateCompletedLaps(Driver driver)
    {
        double completedLaps = 0;
        const double averageFuelConsumptionDeviation = 0.01;

        if (driver.Laps.Count == 0) return 0;

        // (JB) The fuelUsed XML item in the data only becomes available as of the 2024/04/16 update! So before that,
        //      I'll use the very rough laps driven estimate...
        if (!driver.Laps.Exists(lap => lap.FuelUsed > 0)) return driver.Laps.Count - 1;
        
        // (JB) OK, so with the following solution it's necessary for any distance to count that you completed at least
        //      one valid lap.
        // (JB) Another challenge: when doing a 'Return to garage' and you refuel, the next invalid lap has a negative
        //      fuelUsed value, X-).
        // TODO (JB) Implement some sort of car/track average fuel consumption dictionary as a fallback.
        
        var validLaps = driver.Laps.Where(lap => !lap.LapTime.Contains("--.")).ToList();

        if (validLaps.Count > 0)
        {
            completedLaps += validLaps.Count;
            var averageFuelConsumptionPerValidLap = validLaps.Average(lap => lap.FuelUsed);
            
            var invalidLaps = driver.Laps.Where(lap => lap.LapTime.Contains("--.")).ToList();
            double fuelUsedForInvalidLaps = 0;
         
            foreach (Lap invalidLap in invalidLaps)
            {
                // (JB) Laps completed but invalidated are counted here.
                if (invalidLap.FuelUsed > (averageFuelConsumptionPerValidLap - averageFuelConsumptionDeviation) &&
                    invalidLap.FuelUsed < (averageFuelConsumptionPerValidLap + averageFuelConsumptionDeviation))
                {
                    completedLaps++;
                }
                // (JB) Incomplete laps are counted here (e.g. your out lap).
                else if (invalidLap.FuelUsed > 0 && invalidLap.FuelUsed < averageFuelConsumptionPerValidLap + averageFuelConsumptionDeviation)
                {
                    fuelUsedForInvalidLaps += invalidLap.FuelUsed;
                }

            }

            if (fuelUsedForInvalidLaps > 0)
            {
                completedLaps += fuelUsedForInvalidLaps / averageFuelConsumptionPerValidLap;
            }
        }
        
        return completedLaps;
    }
}