CREATE OR REPLACE PROCEDURE CHEMINV_GET_HEADER (
    p_chem_control_number IN VARCHAR2,
    p_output_header OUT VARCHAR2
)
AS
    v_chem_category CHAR(1);
BEGIN
    SELECT ASSET_CLASS 
    INTO v_chem_category
    FROM TV_CHEMINV 
    WHERE CHEM_CONTROL_NUMBER = p_chem_control_number;

    IF v_chem_category IN ('C', 'R') THEN
        DECLARE
            v_assettype_create_initial_receipt CHAR(1);
        BEGIN
            SELECT CREATE_INITIAL_RECEIPT 
            INTO v_assettype_create_initial_receipt
            FROM TV_ASSETTYP
            WHERE CODE = (SELECT ASSET_TYPE FROM TV_CHEMINV WHERE CHEM_CONTROL_NUMBER = p_chem_control_number);

            IF v_assettype_create_initial_receipt = 'T' THEN
                DECLARE
                    v_quantity NUMBER(18,2);
                    v_quantity_units VARCHAR2(50);
                    v_assettyp_description_t VARCHAR2(50);
                    v_lot_number_t VARCHAR2(50);
                BEGIN
                    SELECT NVL(A.QUANTITY, 0),
                           NVL(A.QUANTITY_UNITS, 'Unknown'),
                           NVL(B.DESCRIPTION, 'Unknown'),
                           NVL(A.LOT_NUMBER, 'Unknown')
                    INTO v_quantity, v_quantity_units, v_assettyp_description_t, v_lot_number_t
                    FROM TV_CHEMINV A
                    JOIN TV_ASSETTYP B ON A.ASSET_TYPE = B.CODE
                    WHERE CHEM_CONTROL_NUMBER = p_chem_control_number;

                    p_output_header := TO_CHAR(v_quantity) || ' ' || v_quantity_units || ' of the chemical ' ||
                                       v_assettyp_description_t || ' stored in ' || v_lot_number_t;
                END;
            ELSE
                DECLARE
                    v_assettyp_description VARCHAR2(50);
                    v_lot_number VARCHAR2(50);
                BEGIN
                    SELECT NVL(B.DESCRIPTION, 'Unknown'),
                           NVL(A.LOT_NUMBER, 'Unknown')
                    INTO v_assettyp_description, v_lot_number
                    FROM TV_CHEMINV A
                    JOIN TV_ASSETTYP B ON A.ASSET_TYPE = B.CODE
                    WHERE CHEM_CONTROL_NUMBER = p_chem_control_number;

                    p_output_header := 'A ' || v_assettyp_description || ' chemical stored in ' || v_lot_number;
                END;
            END IF;
        END;
    ELSIF v_chem_category = 'F' THEN
        DECLARE
            v_caliber_description VARCHAR2(50);
            v_manuinfo_name VARCHAR2(50);
            v_instrument_model_number VARCHAR2(50);
            v_assettype_description VARCHAR2(50);
        BEGIN
            SELECT NVL(C.DESCRIPTION, 'Unknown'),
                   NVL(M.NAME, 'Unknown'),
                   NVL(I.INSTRUMENT_MODEL_NUMBER, 'Unknown'),
                   NVL(T.DESCRIPTION, 'Unknown')
            INTO v_caliber_description, v_manuinfo_name, v_instrument_model_number, v_assettype_description
            FROM TV_CHEMINV I
            JOIN TV_MANUINFO M ON I.MANUFACTURER_NAME = M.CODE
            JOIN TV_CALIBER C ON I.CALIBER = C.CODE
            JOIN TV_ASSETTYP T ON I.ASSET_TYPE = T.CODE
            WHERE CHEM_CONTROL_NUMBER = p_chem_control_number;

            p_output_header := 'A ' || v_caliber_description || ' ' || v_manuinfo_name || ' ' ||
                               v_assettype_description || ', model ' || v_instrument_model_number;

            IF LENGTH(p_output_header) > 50 THEN
                p_output_header := SUBSTR(p_output_header, 1, 50) || '...';
            END IF;
        END;
    ELSIF v_chem_category = 'I' THEN
        DECLARE
            v_calinst_next_date DATE;
            v_assettyp_description_instr VARCHAR2(50);
        BEGIN
            SELECT NVL(A.NEXT_DATE, SYSDATE),
                   NVL(C.DESCRIPTION, 'Unknown')
            INTO v_calinst_next_date, v_assettyp_description_instr
            FROM TV_CALINST A
            JOIN TV_CHEMINV B ON A.CHEMICAL_CONTROL_NUMBER = B.CHEM_CONTROL_NUMBER
            JOIN TV_ASSETTYP C ON B.ASSET_CLASS = C.ASSET_CLASS
            WHERE CHEM_CONTROL_NUMBER = p_chem_control_number;

            p_output_header := 'Instrument ' || v_assettyp_description_instr;

            IF v_calinst_next_date < ADD_MONTHS(SYSDATE, 1) THEN
                p_output_header := p_output_header || ' Must be calibrated in ' || TO_CHAR(v_calinst_next_date, 'YYYY-MM-DD');
            END IF;
        END;
    ELSIF v_chem_category = 'D' THEN
        DECLARE
            v_custcode_description VARCHAR2(50);
            v_analyst_name VARCHAR2(50);
        BEGIN
            SELECT NVL(B.DESCRIPTION, 'Unknown'),
                   NVL(C.NAME, 'Unknown')
            INTO v_custcode_description, v_analyst_name
            FROM TV_CHEMINV A
            JOIN TV_CUSTCODE B ON A.CURRENT_STATUS = B.CUSTODY_TYPE
            JOIN TV_ANALYST C ON A.CURRENT_LOCATION = C.ANALYST
            WHERE A.CHEM_CONTROL_NUMBER = p_chem_control_number;

            p_output_header := v_custcode_description || ' - General Storage Lockers >> Analysts - ' || v_analyst_name;
        END;
    ELSE
        DECLARE
            v_type_desc VARCHAR2(50);
            v_asset_type VARCHAR2(50);
            v_barcode VARCHAR2(50);
            v_qty NUMBER(18,2);
        BEGIN
            SELECT NVL(B.DESCRIPTION, 'Unknown'),
                   NVL(A.ASSET_TYPE, 'Unknown'),
                   NVL(A.BARCODE, 'Unknown'),
                   NVL(A.QUANTITY, 0)
            INTO v_type_desc, v_asset_type, v_barcode, v_qty
            FROM TV_CHEMINV A
            JOIN TV_ASSETTYP B ON A.ASSET_TYPE = B.CODE
            WHERE A.CHEM_CONTROL_NUMBER = p_chem_control_number;

            p_output_header := 'Name: ' || v_type_desc || ', Type: ' || v_asset_type ||
                               ', Tracking #: ' || v_barcode || ', Quantity Remaining: ' || TO_CHAR(v_qty) ||
                               ', Images: {null}, Docs: {null}';
        END;
    END IF;
EXCEPTION
    WHEN NO_DATA_FOUND THEN
        p_output_header := 'No data found for control number: ' || p_chem_control_number;
    WHEN OTHERS THEN
        p_output_header := 'Error: ' || SQLERRM;
END CHEMINV_GET_HEADER;
/
