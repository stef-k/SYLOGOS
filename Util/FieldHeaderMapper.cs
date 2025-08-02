using SYLOGOS.Models;

namespace SYLOGOS.Util
{
    public static class FieldHeaderMapper
    {
        private static readonly Dictionary<string, string> _defaultMap = new()
        {
            // Member
            ["Id"] = "ID",
            ["MemberNumber"] = "ΑΡΙΘΜΟΣ ΜΕΛΟΥΣ",
            ["FullName"] = "ΟΝΟΜΑΤΕΠΩΝΥΜΟ",
            ["SpouseFullName"] = "ΟΝΟΜΑ ΣΥΖΥΓΟΥ",
            ["City"] = "ΠΟΛΗ",
            ["Address"] = "ΔΙΕΥΘΥΝΣΗ",
            ["MemberPhone"] = "ΤΗΛΕΦΩΝΟ",
            ["SpousePhone"] = "ΤΗΛΕΦΩΝΟ ΣΥΖΥΓΟΥ",
            ["Email"] = "EMAIL",
            ["CertificateNumber"] = "ΑΡΙΘ. ΠΙΣΤΟΠΟΙΗΤΙΚΟΥ",
            ["CertificatePublisher"] = "ΕΚΔΟΤΗΣ ΠΙΣΤΟΠΟΙΗΤΙΚΟΥ",
            ["Notes"] = "ΣΗΜΕΙΩΣΕΙΣ",
            ["RegistrationDate"] = "ΗΜΕΡΟΜΗΝΙΑ ΕΓΓΡΑΦΗΣ",

            // Child
            ["FullName"] = "ΟΝΟΜΑΤΕΠΩΝΥΜΟ ΤΕΚΝΟΥ",
            ["DateOfBirth"] = "ΗΜ/ΝΙΑ ΓΕΝΝΗΣΗΣ",
            ["MemberId"] = "ID ΜΕΛΟΥΣ",

            // Membership
            ["Year"] = "ΕΤΟΣ",
            ["Amount"] = "ΠΟΣΟ",
            ["ReceiptNumber"] = "ΑΡΙΘΜΟΣ ΑΠΟΔΕΙΞΗΣ",
            ["ReceiptYear"] = "ΕΤΟΣ ΑΠΟΔΕΙΞΗΣ",

            // AppSetting
            ["ClubName"] = "ΟΝΟΜΑ ΣΥΛΛΟΓΟΥ",
            ["Phone"] = "ΤΗΛΕΦΩΝΟ ΣΥΛΛΟΓΟΥ",
            ["Email"] = "EMAIL ΣΥΛΛΟΓΟΥ",
            ["Website"] = "ΙΣΤΟΣΕΛΙΔΑ",
            ["Address"] = "ΔΙΕΥΘΥΝΣΗ ΣΥΛΛΟΓΟΥ",
            ["ReceiptStartNumber"] = "ΑΡΧΙΚΟΣ ΑΡΙΘΜΟΣ ΑΠΟΔΕΙΞΗΣ",
            ["ClubLogo"] = "ΛΟΓΟΤΥΠΟ",
            ["UseDarkMode"] = "ΣΚΟΤΕΙΝΟ ΘΕΜΑ",
            ["ScaleMode"] = "ΚΛΙΜΑΚΑ ΕΜΦΑΝΙΣΗΣ",

            // ReceiptSequence
            ["LastIssuedNumber"] = "ΤΕΛΕΥΤΑΙΟΣ ΑΡΙΘΜΟΣ"
        };

        public static string GetHeader(string fieldName, Type? modelType = null)
        {
            return fieldName == "Address"
                ? modelType == typeof(AppSetting) ? "ΔΙΕΥΘΥΝΣΗ ΣΥΛΛΟΓΟΥ" : "ΔΙΕΥΘΥΝΣΗ"
                : fieldName == "ClubName"
                ? "ΟΝΟΜΑ ΣΥΛΛΟΓΟΥ"
                : fieldName == "Phone" && modelType == typeof(AppSetting)
                ? "ΤΗΛΕΦΩΝΟ ΣΥΛΛΟΓΟΥ"
                : _defaultMap.TryGetValue(fieldName, out string? result)
                ? result
                : fieldName.ToUpperInvariant();
        }

        private static readonly Dictionary<string, string> _queryTitleTemplates = new()
        {
            ["UnpaidMembers"] = "ΜΕΛΗ ΜΕ ΟΦΕΙΛΕΣ",
            ["FullyPaidMembers"] = "ΜΕΛΗ ΜΕ ΠΛΗΡΗ ΕΞΟΦΛΗΣΗ",
            ["PartiallyPaidMembers"] = "ΜΕΛΗ ΜΕ ΜΕΡΙΚΗ ΕΞΟΦΛΗΣΗ",
            ["MembershipsByYear"] = "ΠΛΗΡΩΜΕΣ ΣΥΝΔΡΟΜΩΝ ΓΙΑ ΤΟ ΕΤΟΣ {0}",
            ["TotalPaymentsByYear"] = "ΣΥΝΟΛΟ ΠΛΗΡΩΜΩΝ ΓΙΑ ΤΟ ΕΤΟΣ {0}",
            ["ChildrenWithBirthday"] = "ΤΕΚΝΑ ΜΕ ΓΕΝΕΘΛΙΑ ({0:dd/MM})",
            ["ChildrenAgedBetween"] = "ΤΕΚΝΑ ΗΛΙΚΙΑΣ {0}–{1} ΕΤΩΝ",
            ["ChildrenPerMemberSummary"] = "ΣΥΝΟΨΗ ΤΕΚΝΩΝ ΑΝΑ ΜΕΛΟΣ",
            ["RecentlyRegisteredMembers"] = "ΝΕΕΣ ΕΓΓΡΑΦΕΣ ΤΕΛΕΥΤΑΙΩΝ {0} ΜΗΝΩΝ",
            ["MembersByCity"] = "ΜΕΛΗ ΑΝΑ ΠΟΛΗ {0}",
            ["MembersMissingEmailOrPhone"] = "ΜΕΛΗ ΧΩΡΙΣ EMAIL Ή ΤΗΛΕΦΩΝΟ",
            ["MembersWithCertificate"] = "ΜΕΛΗ ΜΕ ΠΙΣΤΟΠΟΙΗΤΙΚΟ",
            ["MembersWithoutCertificate"] = "ΜΕΛΗ ΧΩΡΙΣ ΠΙΣΤΟΠΟΙΗΤΙΚΟ",
            ["MembersRegisteredInYear"] = "ΜΕΛΗ ΕΓΓΕΓΡΑΜΜΕΝΑ ΤΟ {0}",
            ["PickRandomMembers"] = "ΤΥΧΑΙΑ ΕΠΙΛΟΓΗ {0} ΜΕΛΩΝ",
            ["PickRandomMembersByCity"] = "ΤΥΧΑΙΑ ΕΠΙΛΟΓΗ {0} ΜΕΛΩΝ ΑΠΟ ΠΟΛΗ {1}"
        };


        public static string GetQueryTitle(string key, params object[] args)
        {
            if (_queryTitleTemplates.TryGetValue(key, out string template))
            {
                try
                {
                    return string.Format(template, args);
                }
                catch
                {
                    return template; // fallback if format fails
                }
            }

            return key.Replace('_', ' ').ToUpperInvariant(); // fallback
        }


    }
}
