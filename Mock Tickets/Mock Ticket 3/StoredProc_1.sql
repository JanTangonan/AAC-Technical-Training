CREATE PROCEDURE [dbo].[CHEMINV_GET_HEADER]
    @chem_control_number VARCHAR(10),
    @output_header VARCHAR(50) OUTPUT
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
        WHERE CODE = (SELECT ASSET_CLASS FROM TV_CHEMINV WHERE CHEM_CONTROL_NUMBER = @chem_control_number)

        IF @assettype_create_initial_receipt = 'T'
        BEGIN
            DECLARE @quantity DECIMAL(18),
                    @quantity_units VARCHAR(50)

            SELECT @quantity = QUANTITY,
                   @quantity_units = QUANTITY_UNITS
            FROM TV_CHEMINV
            WHERE CHEM_CONTROL_NUMBER = @chem_control_number

            SET @output_header = CAST(@quantity AS VARCHAR) + ' ' + @quantity_units
        END
        ELSE
        BEGIN
            DECLARE @assettyp_description VARCHAR(50),
                    @lot_number VARCHAR(50)

            SELECT @assettyp_description = B.DESCRIPTION,
                   @lot_number = LOT_NUMBER
            FROM TV_CHEMINV A
            JOIN TV_ASSETTYP B ON A.ASSET_TYPE = B.CODE
            WHERE CHEM_CONTROL_NUMBER = @chem_control_number

            SET @output_header = 'A ' + @assettyp_description + ' chemical stored in ' + @lot_number
        END
    END
    ELSE IF @chem_category = 'F'
    BEGIN
        DECLARE @caliber_description VARCHAR(50),
                @manuinfo_name VARCHAR(50),
                @instrument_model_number VARCHAR(50),
                @assettype_description VARCHAR(50)

        SELECT @caliber_description = C.DESCRIPTION,
               @manuinfo_name = M.NAME,
               @instrument_model_number = I.INSTRUMENT_MODEL_NUMBER,
               @assettype_description = T.DESCRIPTION
        FROM TV_CHEMINV I
        JOIN TV_MANUINFO M ON I.MANUFACTURER_NAME = M.CODE
        JOIN TV_CALIBER C ON I.CALIBER = C.CODE
        JOIN TV_ASSETTYP T ON I.ASSET_TYPE = T.CODE
        WHERE CHEM_CONTROL_NUMBER = @chem_control_number

        SET @output_header = 'A ' + @caliber_description + ' ' + @manuinfo_name + ' ' + @assettype_description + ', model ' + @instrument_model_number
    END
    ELSE IF @chem_category = 'I'
    BEGIN
        DECLARE @calinst_next_date DATE,
                @assettyp_description_instr VARCHAR(50)

        SELECT @calinst_next_date = A.NEXT_DATE,
               @assettyp_description_instr = C.DESCRIPTION
        FROM TV_CALINST A
        JOIN TV_CHEMINV B ON A.CHEMICAL_CONTROL_NUMBER = B.CHEM_CONTROL_NUMBER
        JOIN TV_ASSETTYP C ON B.ASSET_CLASS = C.ASSET_CLASS
        WHERE CHEM_CONTROL_NUMBER = @chem_control_number

        SET @output_header = 'Instrument ' + @assettyp_description

        IF @calinst_next_date < DATEADD(MONTH, 1, GETDATE())
        BEGIN
            SET @output_header += ' Must be calibrated in ' + CAST(@calinst_next_date AS VARCHAR)
        END
    END
    ELSE IF @chem_category = 'D'
    BEGIN
        DECLARE @custcode_description VARCHAR(50),
                @analyst_name VARCHAR(50)

        SELECT @custcode_description = B.DESCRIPTION,
               @analyst_name = C.NAME
        FROM TV_CHEMINV A
        JOIN TV_CUSTCODE B ON A.CURRENT_STATUS = B.CUSTODY_TYPE
        JOIN TV_ANALYST C ON A.CURRENT_LOCATION = C.ANALYST
        WHERE A.CHEM_CONTROL_NUMBER = @chem_control_number

        SET @output_header = @custcode_description + ' - General Storage Lockers >> Analysts - ' + @analyst_name
    END
    ELSE
    BEGIN
        DECLARE @type_desc VARCHAR(50),
                @asset_type VARCHAR(50),
                @barcode VARCHAR(50),
                @qty DECIMAL(18)

        SELECT @type_desc = B.DESCRIPTION,
               @asset_type = A.ASSET_TYPE,
               @barcode = A.BARCODE,
               @qty = A.QUANTITY
        FROM TV_CHEMINV A
        JOIN TV_ASSETTYP B ON A.ASSET_TYPE = B.CODE
        WHERE A.CHEM_CONTROL_NUMBER = @chem_control_number

        SET @output_header = 'Name: ' + @type_desc + ', Type: ' + @asset_type + ', Tracking #: ' + @barcode + ', Quantity Remaining: ' + CAST(@qty AS VARCHAR) + ', Images: {null}, Docs: {null}'
    END
END