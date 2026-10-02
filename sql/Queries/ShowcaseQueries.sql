/* ---------------------------------------------------------------------------
   Example analytical queries on the loaded data (not run by the importer).
   --------------------------------------------------------------------------- */

-- 1) One pair's timeline (Russia = 365, Iran = 630)
EXEC dbo.usp_GetPairTimeline @CcodeA = 365, @CcodeB = 630;

-- 2) Longest alliances that have ended (pair level), with country names
SELECT TOP (20)
       c1.Name AS CountryA,
       c2.Name AS CountryB,
       e.Source,
       e.Title,
       e.StartYear,
       e.EndYear,
       e.EndYear - e.StartYear AS Years
FROM dbo.DyadEvent AS e
JOIN dbo.Country AS c1 ON c1.Ccode = e.CcodeLo
JOIN dbo.Country AS c2 ON c2.Ccode = e.CcodeHi
WHERE e.EventType = 'alliance'
  AND e.EndYear IS NOT NULL
ORDER BY Years DESC, e.StartYear;

-- 3) Country-pair wars per decade
SELECT (StartYear / 10) * 10 AS Decade,
       COUNT(*)              AS PairWars
FROM dbo.DyadEvent
WHERE EventType = 'war'
GROUP BY (StartYear / 10) * 10
ORDER BY Decade;

-- 4) Countries ranked by number of inter-state war participations (window function)
SELECT c.Name,
       COUNT(*)                            AS WarParticipations,
       RANK() OVER (ORDER BY COUNT(*) DESC) AS Rnk
FROM dbo.CountryWar AS w
JOIN dbo.Country    AS c ON c.Ccode = w.Ccode
GROUP BY c.Name
ORDER BY WarParticipations DESC, c.Name;

-- 5) Pairs that were allied AND later fought each other
SELECT DISTINCT c1.Name AS CountryA, c2.Name AS CountryB
FROM dbo.DyadEvent AS a
JOIN dbo.DyadEvent AS w
     ON w.CcodeLo = a.CcodeLo AND w.CcodeHi = a.CcodeHi
    AND w.EventType = 'war'
    AND w.SortKey > a.SortKey
JOIN dbo.Country AS c1 ON c1.Ccode = a.CcodeLo
JOIN dbo.Country AS c2 ON c2.Ccode = a.CcodeHi
WHERE a.EventType = 'alliance'
ORDER BY CountryA, CountryB;

-- 6) Leaders in office when a war between two countries started (pilot countries only)
SELECT e.Title, l.Name AS Leader, c.Name AS Country
FROM dbo.DyadEvent AS e
JOIN dbo.Leader    AS l ON l.Ccode IN (e.CcodeLo, e.CcodeHi)
                       AND DATEFROMPARTS(e.StartYear, ISNULL(e.StartMonth, 1), ISNULL(e.StartDay, 1))
                           BETWEEN l.StartDate AND l.EndDate
JOIN dbo.Country   AS c ON c.Ccode = l.Ccode
WHERE e.EventType = 'war'
ORDER BY e.SortKey, c.Name;
