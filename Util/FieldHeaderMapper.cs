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
            ["MemberName"] = "ΟΝΟΜΑΤΕΠΩΝΥΜΟ",
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
            // Fix 1: Disambiguate by field + model class explicitly
            if (fieldName == "FullName")
            {
                if (modelType == typeof(Child) || modelType?.Name.Contains("Child") == true)
                {
                    return "ΟΝΟΜΑΤΕΠΩΝΥΜΟ ΤΕΚΝΟΥ";
                }

                return "ΟΝΟΜΑΤΕΠΩΝΥΜΟ"; // fallback to Member
            }

            if (fieldName == "Email")
            {
                return modelType == typeof(AppSetting) ? "EMAIL ΣΥΛΛΟΓΟΥ" : "EMAIL";
            }

            if (fieldName == "Address")
            {
                return modelType == typeof(AppSetting) ? "ΔΙΕΥΘΥΝΣΗ ΣΥΛΛΟΓΟΥ" : "ΔΙΕΥΘΥΝΣΗ";
            }

            // Fix 2: Catch common DTO cases
            if (fieldName == "Children")
            {
                return "ΤΕΚΝΑ";
            }

            if (fieldName == "MemberName")
            {
                return "ΟΝΟΜΑΤΕΠΩΝΥΜΟ";
            }

            // Default fallback
            return _defaultMap.TryGetValue(fieldName, out string? result)
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
            ["ChildrenWithBirthday"] = "ΤΕΚΝΑ ΜΕ ΓΕΝΕΘΛΙΑ ({0:dd/MM/yyyy})",
            ["ChildrenAgedBetween"] = "ΤΕΚΝΑ ΗΛΙΚΙΑΣ {0}–{1} ΕΤΩΝ",
            ["ChildrenPerMemberSummary"] = "ΣΥΝΟΨΗ ΤΕΚΝΩΝ ΑΝΑ ΜΕΛΟΣ",
            ["RecentlyRegisteredMembers"] = "ΝΕΕΣ ΕΓΓΡΑΦΕΣ ΤΕΛΕΥΤΑΙΩΝ {0} ΜΗΝΩΝ",
            ["MembersByCity"] = "ΜΕΛΗ ΑΝΑ ΠΟΛΗ {0}",
            ["MembersMissingEmailOrPhone"] = "ΜΕΛΗ ΧΩΡΙΣ EMAIL Ή ΤΗΛΕΦΩΝΟ",
            ["MembersWithCertificate"] = "ΜΕΛΗ ΜΕ ΠΙΣΤΟΠΟΙΗΤΙΚΟ",
            ["MembersWithoutCertificate"] = "ΜΕΛΗ ΧΩΡΙΣ ΠΙΣΤΟΠΟΙΗΤΙΚΟ",
            ["MembersRegisteredInYear"] = "ΜΕΛΗ ΕΓΓΕΓΡΑΜΜΕΝΑ ΤΟ {0}",
            ["PickRandomMembers"] = "{0} ({1} ΜΕΛΗ)",
            ["PickRandomMembersByCity"] = "{0} ({1} ΜΕΛΗ ΑΠΟ ΠΟΛΗ {2})"
        };


        public static string GetQueryTitle(string key, params object[] args)
        {
            if (_queryTitleTemplates.TryGetValue(key, out string template))
            {
                try
                {
                    // Handle PickRandomMembers with fallback
                    if (key == "PickRandomMembers" && args.Length == 2)
                    {
                        string title = string.IsNullOrWhiteSpace(args[0]?.ToString()) ? "ΤΥΧΑΙΑ ΕΠΙΛΟΓΗ" : args[0].ToString()!;
                        return string.Format("{0} ({1} ΜΕΛΗ)", title, args[1]);
                    }

                    if (key == "PickRandomMembersByCity" && args.Length == 3)
                    {
                        string title = string.IsNullOrWhiteSpace(args[0]?.ToString()) ? "ΤΥΧΑΙΑ ΕΠΙΛΟΓΗ" : args[0].ToString()!;
                        return string.Format("{0} ({1} ΜΕΛΗ ΑΠΟ ΠΟΛΗ {2})", title, args[1], args[2]);
                    }

                    return string.Format(template, args);
                }
                catch
                {
                    return template;
                }
            }

            return key.Replace('_', ' ').ToUpperInvariant();
        }

    }
}
