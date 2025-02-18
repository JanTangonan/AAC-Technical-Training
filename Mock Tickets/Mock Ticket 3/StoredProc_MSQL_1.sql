USE [VENTURA_DEV]
GO
/****** Object:  StoredProcedure [dbo].[CHEMINV_GET_HEADER]    Script Date: 2/17/2025 2:31:27 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER PROCEDURE [dbo].[CHEMINV_GET_HEADER]
    @chem_control_number VARCHAR(10),
    @output_header VARCHAR(100) OUTPUT
AS
BEGIN
    DECLARE @chem_category CHAR(1)

    SELECT @chem_category = ASSET_CLASS
    FROM TV_CHEMINV
    WHERE CHEM_CONTROL_NUMBER = @chem_control_number

    IF @chem_category IN ('C', 'R')
    BEGIN
        DECLARE @assettype_create_initial_receipt CHAR(1)

        SELECT @assettype_create_initial_receipt = CREATE_INITIAL_RECEIPT
        FROM TV_ASSETTYP
        WHERE CODE = (SELECT ASSET_TYPE FROM TV_CHEMINV WHERE CHEM_CONTROL_NUMBER = @chem_control_number)

        IF @assettype_create_initial_receipt = 'T'
        BEGIN
            DECLARE @quantity DECIMAL(18),
                    @quantity_units VARCHAR(50),
					@assettyp_description_t VARCHAR(50),
                    @lot_number_t VARCHAR(50)

            SELECT @quantity = ISNULL(A.QUANTITY, 0),
                   @quantity_units = ISNULL(A.QUANTITY_UNITS, 'Unknown'),
				   @assettyp_description_t = ISNULL(B.DESCRIPTION, 'Unknown'),
                   @lot_number_t = ISNULL(LOT_NUMBER, 'Unknown')
            FROM TV_CHEMINV A
			JOIN TV_ASSETTYP B ON A.ASSET_TYPE = B.CODE
            WHERE CHEM_CONTROL_NUMBER = @chem_control_number

			-- check if empty query result
			IF @quantity IS NULL AND @quantity_units IS NULL AND @assettyp_description_t IS NULL AND @lot_number_t IS NULL
            BEGIN
                SET @output_header = 'No data found for control number: ' + @chem_control_number;
                RETURN;
            END

            SET @output_header = CAST(@quantity AS VARCHAR) + ' ' + @quantity_units + ' of the chemical ' + @assettyp_description_t + ' stored in ' + @lot_number_t
        END
        ELSE
        BEGIN
            DECLARE @assettyp_description VARCHAR(50),
                    @lot_number VARCHAR(50)

            SELECT @assettyp_description = ISNULL(B.DESCRIPTION, 'Unknown'),
                   @lot_number = ISNULL(LOT_NUMBER,'Unknown')
            FROM TV_CHEMINV A
            JOIN TV_ASSETTYP B ON A.ASSET_TYPE = B.CODE
            WHERE CHEM_CONTROL_NUMBER = @chem_control_number

			-- check if empty query result
			IF @assettyp_description IS NULL AND @lot_number IS NULL
            BEGIN
                SET @output_header = 'No data found for control number: ' + @chem_control_number;
                RETURN;
            END

            SET @output_header = 'A ' + @assettyp_description + ' chemical stored in ' + @lot_number
        END
    END
    ELSE IF @chem_category = 'F'
    BEGIN
        DECLARE @caliber_description VARCHAR(50),
                @manuinfo_name VARCHAR(50),
                @instrument_model_number VARCHAR(50),
                @assettype_description VARCHAR(50)

        SELECT @caliber_description = ISNULL(C.DESCRIPTION, 'Unknown'),
               @manuinfo_name = ISNULL(M.NAME, 'Unknown'),
               @instrument_model_number = ISNULL(I.INSTRUMENT_MODEL_NUMBER,'Unknown'),
               @assettype_description = ISNULL(T.DESCRIPTION, 'Unknown')
        FROM TV_CHEMINV I
        JOIN TV_MANUINFO M ON I.MANUFACTURER_NAME = M.CODE
        JOIN TV_CALIBER C ON I.CALIBER = C.CODE
        JOIN TV_ASSETTYP T ON I.ASSET_TYPE = T.CODE
        WHERE CHEM_CONTROL_NUMBER = @chem_control_number

		-- check if empty query result
		IF @caliber_description IS NULL AND @manuinfo_name IS NULL AND @instrument_model_number IS NULL AND @assettype_description IS NULL
        BEGIN
            SET @output_header = 'No data found for control number: ' + @chem_control_number;
            RETURN;
        END

        SET @output_header = 'A ' + @caliber_description + ' ' + @manuinfo_name + ' ' + @assettype_description + ', model ' + @instrument_model_number
    END
    ELSE IF @chem_category = 'I'
    BEGIN
        DECLARE @calinst_next_date DATE,
                @assettyp_description_instr VARCHAR(50)

        SELECT @calinst_next_date = ISNULL(A.NEXT_DATE, GETDATE()),
               @assettyp_description_instr = ISNULL(C.DESCRIPTION, 'Unknown')
        FROM TV_CALINST A
        JOIN TV_CHEMINV B ON A.CHEMICAL_CONTROL_NUMBER = B.CHEM_CONTROL_NUMBER
        JOIN TV_ASSETTYP C ON B.ASSET_CLASS = C.ASSET_CLASS
        WHERE CHEM_CONTROL_NUMBER = @chem_control_number
		
		-- check if empty query result
		IF @calinst_next_date IS NULL AND @assettyp_description_instr IS NULL
		BEGIN
			SET @output_header = 'No data found for control number: ' + @chem_control_number;
			RETURN;
		END

        SET @output_header = 'Instrument ' + @assettyp_description_instr

        IF @calinst_next_date < DATEADD(MONTH, 1, GETDATE())
        BEGIN
            SET @output_header += ' Must be calibrated in ' + CONVERT(VARCHAR, @calinst_next_date, 120)
        END
    END
    ELSE IF @chem_category = 'D'
    BEGIN
        DECLARE @custcode_description VARCHAR(50),
                @analyst_name VARCHAR(50)

        SELECT @custcode_description = ISNULL(B.DESCRIPTION, 'Unknown'),
               @analyst_name = ISNULL(C.NAME, 'Unknown')
        FROM TV_CHEMINV A
        JOIN TV_CUSTCODE B ON A.CURRENT_STATUS = B.CUSTODY_TYPE
        JOIN TV_ANALYST C ON A.CURRENT_LOCATION = C.ANALYST
        WHERE A.CHEM_CONTROL_NUMBER = @chem_control_number

		-- check if empty query result
		IF @custcode_description IS NULL AND @analyst_name IS NULL
        BEGIN
            SET @output_header = 'No data found for control number: ' + @chem_control_number;
            RETURN;
        END

        SET @output_header = @custcode_description + ' - General Storage Lockers >> Analysts - ' + @analyst_name
		IF LEN(@output_header) > 49
        BEGIN
            SET @output_header = LEFT(@output_header, 99) + '...'
        END
    END
    ELSE
    BEGIN
        DECLARE @type_desc VARCHAR(50),
                @asset_type VARCHAR(50),
                @barcode VARCHAR(50),
                @qty DECIMAL(18)

        SELECT @type_desc = ISNULL(B.DESCRIPTION, 'Unknown'),
               @asset_type = ISNULL(A.ASSET_TYPE, 'Unknown'),
               @barcode = ISNULL(A.BARCODE, 'Unknown'),
               @qty = ISNULL(A.QUANTITY, 0)
        FROM TV_CHEMINV A
        JOIN TV_ASSETTYP B ON A.ASSET_TYPE = B.CODE
        WHERE A.CHEM_CONTROL_NUMBER = @chem_control_number

		-- check if empty query result
		IF @type_desc IS NULL AND @asset_type IS NULL AND @barcode IS NULL AND @qty IS NULL
        BEGIN
            SET @output_header = 'No data found for control number: ' + @chem_control_number;
            RETURN;
        END

        SET @output_header = 'Name: ' + @type_desc + ', Type: ' + @asset_type + ', Tracking #: ' + @barcode + ', Quantity Remaining: ' + CAST(@qty AS VARCHAR)
    END
END