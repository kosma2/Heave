IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'GetGraphData')
    DROP PROCEDURE GetGraphData;
GO
CREATE PROCEDURE GetGraphData
    @CustomerId INT
AS
BEGIN
    -- Declare geography point for obstacle
    DECLARE @obstPoint GEOGRAPHY = GEOGRAPHY::STPointFromText('POINT(-117.2904677012464 49.49196670168381)', 4326);
        -- Create buffer around obstacle point
    DECLARE @obst GEOGRAPHY = @obstPoint.STBuffer(300);
        -- Declare variables for Customer's GeoLocation and MarkerName
    DECLARE @CustomerGeo GEOGRAPHY;
    DECLARE @CustomerMarker NVARCHAR(50);

    SELECT
        @CustomerGeo = GeoPoint,
        @CustomerMarker = CAST(CustomerId AS NVARCHAR(50))
    FROM
        Customer
    WHERE
        CustomerId = @CustomerId;

    DECLARE @TempNodes TABLE (
        ID INT,
        MarkerName NVARCHAR(50),
        GeoLocation GEOGRAPHY,
        ShapeName NVARCHAR(50)
    );

    IF @CustomerGeo IS NOT NULL
    BEGIN
        INSERT INTO @TempNodes (ID, MarkerName, GeoLocation, ShapeName)
        VALUES (@CustomerId,@CustomerMarker, @CustomerGeo, 'point');
    END

    CREATE TABLE #Edges (
        StartNodeMarkerName NVARCHAR(50),
        EndNodeMarkerName NVARCHAR(50),
        Distance FLOAT
    );

    INSERT INTO #Edges (StartNodeMarkerName, EndNodeMarkerName, Distance)
    SELECT
        A.MarkerName AS StartNodeMarkerName,
        B.MarkerName AS EndNodeMarkerName,
        A.GeoLocation.STDistance(B.GeoLocation) AS Distance
    FROM
        (SELECT MarkerName, GeoLocation FROM airmarker UNION ALL SELECT MarkerName, GeoLocation FROM @TempNodes) A,
        (SELECT MarkerName, GeoLocation FROM airmarker UNION ALL SELECT MarkerName, GeoLocation FROM @TempNodes) B
    WHERE
        A.MarkerName != B.MarkerName AND
        ROUND(A.GeoLocation.STDistance(B.GeoLocation), 2) < 1800 AND
        GEOGRAPHY::STLineFromText(
            'LINESTRING(' +
            CAST(A.GeoLocation.Long AS VARCHAR(20)) + ' ' +
            CAST(A.GeoLocation.Lat AS VARCHAR(20)) + ', ' +
            CAST(B.GeoLocation.Long AS VARCHAR(20)) + ' ' +
            CAST(B.GeoLocation.Lat AS VARCHAR(20)) +
            ')', 4326
        ).STIntersects(@obst) = 0;

    SELECT
        ID,
        MarkerName,
        GeoLocation.STAsText()
    FROM
        (SELECT ID, MarkerName, GeoLocation FROM airmarker UNION ALL SELECT ID, MarkerName, GeoLocation FROM @TempNodes) AS CombinedNodes;

    SELECT
        StartNodeMarkerName,
        EndNodeMarkerName,
        Distance
    FROM
        #Edges;

    DROP TABLE #Edges;
END;
GO
GRANT EXECUTE ON dbo.GetGraphData TO kosma;
GO
