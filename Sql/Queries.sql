USE NuxibaDB;
GO


-- 1. Usuario con mayor tiempo total logueado


WITH Movimientos AS
(
    SELECT
        User_id,
        TipoMov,
        fecha,

        LEAD(TipoMov) OVER (
            PARTITION BY User_id
            ORDER BY fecha
        ) AS SiguienteTipoMov,

        LEAD(fecha) OVER (
            PARTITION BY User_id
            ORDER BY fecha
        ) AS FechaLogout

    FROM dbo.ccloglogin
),
Sesiones AS
(
    SELECT
        User_id,
        DATEDIFF_BIG(SECOND, fecha, FechaLogout) AS SegundosSesion

    FROM Movimientos

    WHERE TipoMov = 1
      AND SiguienteTipoMov = 0
      AND FechaLogout IS NOT NULL
),
Totales AS
(
    SELECT
        User_id,
        SUM(SegundosSesion) AS TotalSegundos

    FROM Sesiones

    GROUP BY User_id
)

SELECT TOP 1
    User_id,
    TotalSegundos,

    CONCAT(
        TotalSegundos / 86400, ' días, ',
        (TotalSegundos % 86400) / 3600, ' horas, ',
        (TotalSegundos % 3600) / 60, ' minutos, ',
        TotalSegundos % 60, ' segundos'
    ) AS TiempoTotal

FROM Totales

ORDER BY TotalSegundos DESC, User_id ASC;



-- 2. Usuario con menor tiempo total logueado


WITH Movimientos AS
(
    SELECT
        User_id,
        TipoMov,
        fecha,

        LEAD(TipoMov) OVER (
            PARTITION BY User_id
            ORDER BY fecha
        ) AS SiguienteTipoMov,

        LEAD(fecha) OVER (
            PARTITION BY User_id
            ORDER BY fecha
        ) AS FechaLogout

    FROM dbo.ccloglogin
),
Sesiones AS
(
    SELECT
        User_id,
        DATEDIFF_BIG(SECOND, fecha, FechaLogout) AS SegundosSesion

    FROM Movimientos

    WHERE TipoMov = 1
      AND SiguienteTipoMov = 0
      AND FechaLogout IS NOT NULL
),
Totales AS
(
    SELECT
        User_id,
        SUM(SegundosSesion) AS TotalSegundos

    FROM Sesiones

    GROUP BY User_id
)

SELECT TOP 1
    User_id,
    TotalSegundos,

    CONCAT(
        TotalSegundos / 86400, ' días, ',
        (TotalSegundos % 86400) / 3600, ' horas, ',
        (TotalSegundos % 3600) / 60, ' minutos, ',
        TotalSegundos % 60, ' segundos'
    ) AS TiempoTotal

FROM Totales

ORDER BY TotalSegundos ASC, User_id ASC;



-- 3. Promedio de tiempo de logueo por usuario y mes


WITH Movimientos AS
(
    SELECT
        User_id,
        TipoMov,
        fecha,

        LEAD(TipoMov) OVER (
            PARTITION BY User_id
            ORDER BY fecha
        ) AS SiguienteTipoMov,

        LEAD(fecha) OVER (
            PARTITION BY User_id
            ORDER BY fecha
        ) AS FechaLogout

    FROM dbo.ccloglogin
),
Sesiones AS
(
    SELECT
        User_id,
        YEAR(fecha) AS [AÑO],
        MONTH(fecha) AS [MES],
        DATEDIFF_BIG(SECOND, fecha, FechaLogout) AS SegundosSesion

    FROM Movimientos

    WHERE TipoMov = 1
      AND SiguienteTipoMov = 0
      AND FechaLogout IS NOT NULL
),
PromediosMensuales AS
(
    SELECT
        User_id,
        [AÑO],
        [MES],
        AVG(CAST(SegundosSesion AS DECIMAL(19, 2))) AS PromedioSegundos

    FROM Sesiones

    GROUP BY
        User_id,
        [AÑO],
        [MES]
)

SELECT
    User_id,
    [AÑO],
    [MES],
    PromedioSegundos,

    CONCAT(
        CAST(PromedioSegundos AS BIGINT) / 86400, ' días, ',
        (CAST(PromedioSegundos AS BIGINT) % 86400) / 3600, ' horas, ',
        (CAST(PromedioSegundos AS BIGINT) % 3600) / 60, ' minutos, ',
        CAST(PromedioSegundos AS BIGINT) % 60, ' segundos'
    ) AS TiempoPromedio

FROM PromediosMensuales

ORDER BY
    User_id,
    [AÑO],
    [MES];