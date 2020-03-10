USE [ADI_Enrichment]
GO
/****** Object:  StoredProcedure [dbo].[GetMappingOrphans]    Script Date: 09/03/2020 17:35:41 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[GetMappingOrphans] 
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    SELECT 
		*
	FROM GN_Mapping_Data AS G 
	WHERE NOT EXISTS ( 
		SELECT 
			TITLPAID 
		FROM Adi_Data AS A 
		WHERE 
			RIGHT(A.TITLPAID,8) = RIGHT(G.GN_Paid,8)
	);
END
