USE [ADI_Enrichment]
GO
/****** Object:  StoredProcedure [dbo].[GetAdiDataOrphans]    Script Date: 09/03/2020 17:35:13 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE [dbo].[GetAdiDataOrphans]

AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	SELECT
		*
	FROM Adi_Data AS A 
	WHERE NOT EXISTS( 
		SELECT 
			GN_Paid 
		FROM GN_Mapping_Data AS G 
		WHERE 
			RIGHT(G.GN_Paid, 8) = RIGHT(A.TITLPAID, 8)
	);
END
