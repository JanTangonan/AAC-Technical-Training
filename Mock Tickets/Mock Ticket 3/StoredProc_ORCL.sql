CREATE OR REPLACE PROCEDURE CHEMINV_GET_HEADER (
    chem_control_number IN VARCHAR2,
    output_header OUT VARCHAR2
) AS
    chem_category CHAR(1);
BEGIN
    -- Get chem_category
    SELECT ASSET_CLASS INTO chem_category 
    FROM TV_CHEMINV 
    WHERE CHEM_CONTROL_NUMBER = chem_control_number;

    IF chem_category IN ('C', 'R') THEN
        DECLARE
            assettype_create_initial_receipt CHAR(1);
        BEGIN
            -- Get assettype_create_initial_receipt
            SELECT CREATE_INITIAL_RECEIPT INTO assettype_create_initial_receipt
            FROM TV_ASSETTYP
            WHERE CODE = (SELECT ASSET_TYPE FROM TV_CHEMINV WHERE CHEM_CONTROL_NUMBER = chem_control_number);

            IF assettype_create_initial_receipt = 'T' THEN
                DECLARE
                    quantity NUMBER;
                    quantity_units VARCHAR2(50);
                    assettyp_description_t VARCHAR2(50);
                    lot_number_t VARCHAR2(50);
                BEGIN
                    -- Get chemical details
                    SELECT NVL(A.QUANTITY, 0), 
                           NVL(A.QUANTITY_UNITS, 'Unknown'),
                           NVL(B.DESCRIPTION, 'Unknown'),
                           NVL(A.LOT_NUMBER, 'Unknown')
                    INTO quantity, quantity_units, assettyp_description_t, lot_number_t
                    FROM TV_CHEMINV A
                    JOIN TV_ASSETTYP B ON A.ASSET_TYPE = B.CODE
                    WHERE CHEM_CONTROL_NUMBER = chem_control_number;

                    output_header := TO_CHAR(quantity) || ' ' || quantity_units || 
                                     ' of the chemical ' || assettyp_description_t || 
                                     ' stored in ' || lot_number_t;
                END;
            ELSE
                DECLARE
                    assettyp_description VARCHAR2(50);
                    lot_number VARCHAR2(50);
                BEGIN
                    -- Get chemical description and lot number
                    SELECT NVL(B.DESCRIPTION, 'Unknown'),
                           NVL(A.LOT_NUMBER, 'Unknown')
                    INTO assettyp_description, lot_number
                    FROM TV_CHEMINV A
                    JOIN TV_ASSETTYP B ON A.ASSET_TYPE = B.CODE
                    WHERE CHEM_CONTROL_NUMBER = chem_control_number;

                    output_header := 'A ' || assettyp_description || ' chemical stored in ' || lot_number;
                END;
            END IF;
        END;

    ELSIF chem_category = 'F' THEN
        DECLARE
            caliber_description VARCHAR2(50);
            manuinfo_name VARCHAR2(50);
            instrument_model_number VARCHAR2(50);
            assettype_description VARCHAR2(50);
        BEGIN
            -- Get firearm details
            SELECT NVL(C.DESCRIPTION, 'Unknown'),
                   NVL(M.NAME, 'Unknown'),
                   NVL(I.INSTRUMENT_MODEL_NUMBER, 'Unknown'),
                   NVL(T.DESCRIPTION, 'Unknown')
            INTO caliber_description, manuinfo_name, instrument_model_number, assettype_description
            FROM TV_CHEMINV I
            JOIN TV_MANUINFO M ON I.MANUFACTURER_NAME = M.CODE
            JOIN TV_CALIBER C ON I.CALIBER = C.CODE
            JOIN TV_ASSETTYP T ON I.ASSET_TYPE = T.CODE
            WHERE CHEM_CONTROL_NUMBER = chem_control_number;

            output_header := 'A ' || caliber_description || ' ' || manuinfo_name || ' ' || 
                             assettype_description || ', model ' || instrument_model_number;
        END;

    ELSIF chem_category = 'I' THEN
        DECLARE
            calinst_next_date DATE;
            assettyp_description_instr VARCHAR2(50);
        BEGIN
            -- Get instrument details
            SELECT NVL(A.NEXT_DATE, SYSDATE),
                   NVL(C.DESCRIPTION, 'Unknown')
            INTO calinst_next_date, assettyp_description_instr
            FROM TV_CALINST A
            JOIN TV_CHEMINV B ON A.CHEMICAL_CONTROL_NUMBER = B.CHEM_CONTROL_NUMBER
            JOIN TV_ASSETTYP C ON B.ASSET_CLASS = C.ASSET_CLASS
            WHERE CHEM_CONTROL_NUMBER = chem_control_number;

            output_header := 'Instrument ' || assettyp_description_instr;

            -- Check if calibration is due within 1 month
            IF calinst_next_date < ADD_MONTHS(SYSDATE, 1) THEN
                output_header := output_header || ' Must be calibrated in ' || TO_CHAR(calinst_next_date, 'YYYY-MM-DD');
            END IF;
        END;

    ELSIF chem_category = 'D' THEN
        DECLARE
            custcode_description VARCHAR2(50);
            analyst_name VARCHAR2(50);
        BEGIN
            -- Get custody details
            SELECT NVL(B.DESCRIPTION, 'Unknown'),
                   NVL(C.NAME, 'Unknown')
            INTO custcode_description, analyst_name
            FROM TV_CHEMINV A
            JOIN TV_CUSTCODE B ON A.CURRENT_STATUS = B.CUSTODY_TYPE
            JOIN TV_ANALYST C ON A.CURRENT_LOCATION = C.ANALYST
            WHERE A.CHEM_CONTROL_NUMBER = chem_control_number;

            output_header := custcode_description || ' - General Storage Lockers >> Analysts - ' || analyst_name;
        END;

    ELSE
        DECLARE
            type_desc VARCHAR2(50);
            asset_type VARCHAR2(50);
            barcode VARCHAR2(50);
            qty NUMBER;
        BEGIN
            -- Get default asset details
            SELECT NVL(B.DESCRIPTION, 'Unknown'),
                   NVL(A.ASSET_TYPE, 'Unknown'),
                   NVL(A.BARCODE, 'Unknown'),
                   NVL(A.QUANTITY, 0)
            INTO type_desc, asset_type, barcode, qty
            FROM TV_CHEMINV A
            JOIN TV_ASSETTYP B ON A.ASSET_TYPE = B.CODE
            WHERE A.CHEM_CONTROL_NUMBER = chem_control_number;

            output_header := 'Name: ' || type_desc || ', Type: ' || asset_type || 
                             ', Tracking #: ' || barcode || ', Quantity Remaining: ' || TO_CHAR(qty) || 
                             ', Images: {null}, Docs: {null}';
        END;
    END IF;
END CHEMINV_GET_HEADER;
/
