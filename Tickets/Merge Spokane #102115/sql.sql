Select * FROM TV_LABSTAT where STATUS_CODE IS NOT NULL and EVIDENCE_CONTROL_NUMBER = 5884172 ORDER BY STATUS_DATE DESC ,  to_char(STATUS_TIME, 'HH24:MI:SS') DESC, STATUS_KEY DESC

Select * FROM TV_LABSTAT where STATUS_CODE IS NOT NULL and EVIDENCE_CONTROL_NUMBER = 5884172 ORDER BY STATUS_DATE DESC ,  to_char(STATUS_TIME, 'HH24:MI:SS') DESC, STATUS_KEY DESC

if ((previousNeighborValue == _items[counter].NeighboringValue && PLCSession.GetLabCtrlFlag("ALLOW_SAME_LOCATION_TRANSFER") != "T") && counter != 0)

public bool HasSameNeighboringValue()
        {
            string previousNeighborValue = string.Empty;
            for (int counter = 0; counter < _items.Count; counter++)
            {
                if ((previousNeighborValue == _items[counter].NeighboringValue && PLCSession.GetLabCtrlFlag("ALLOW_SAME_LOCATION_TRANSFER") != "T") && counter != 0)
                    return true;
                previousNeighborValue = _items[counter].NeighboringValue;
            }

            return false;
        }

Select * FROM TV_LABSTAT where STATUS_CODE IS NOT NULL and EVIDENCE_CONTROL_NUMBER = 1500682 ORDER BY STATUS_DATE DESC ,  convert(char(8), STATUS_TIME, 108) DESC, STATUS_KEY DESC
"ESS-MB5"
"SCS-MIKE"
"C-NAHIDA"