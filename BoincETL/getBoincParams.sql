BEGIN

(
SELECT 'D:\\ProgramData\\BOINC\\statistics_boinc.bakerlab.org_rosetta.xml' AS 'filename', COALESCE(MAX(`day`), CURDATE()) AS 'LastDates', projectSource
FROM boinc_data
WHERE projectSource = 'Rosetta') UNION (
SELECT 'D:\\ProgramData\\BOINC\\statistics_www.worldcommunitygrid.org.xml', COALESCE(MAX(`day`), CURDATE()), projectSource
FROM boinc_data
WHERE projectSource = 'World Community Grid') UNION (
SELECT 'D:\\ProgramData\\BOINC\\statistics_www.gpugrid.net.xml', COALESCE(MAX(`day`), CURDATE()), projectSource
FROM boinc_data
WHERE projectSource = 'GPUGrid'); END