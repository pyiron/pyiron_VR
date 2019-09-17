using UnityEngine;
using System.Collections.Generic;

// component of Settings
public class LocalElementData : MonoBehaviour {

    // symbol -> CAS number, name, ordinal number
    public static readonly Dictionary<string, Element> m_localElementDict = new Dictionary<string, Element> {
        {  "H", new Element( "1333-74-0",      "Hydrogen",   1,   1f, new Color(255 / 255f, 255 / 255f, 255 / 255f)) },
        { "He", new Element( "7440-59-7",        "Helium",   2, 1.5f, new Color(217 / 255f, 255 / 255f, 255 / 255f)) },
        { "Li", new Element( "7439-93-2",       "Lithium",   3, 1.5f, new Color(204 / 255f, 128 / 255f, 255 / 255f)) },
        { "Be", new Element( "7440-41-7",     "Beryllium",   4, 1.5f, new Color(194 / 255f, 255 / 255f,   0 / 255f)) },
        {  "B", new Element( "7440-42-8",         "Boron",   5, 1.5f, new Color(255 / 255f, 181 / 255f, 181 / 255f)) },
        {  "C", new Element( "7440-44-0",        "Carbon",   6,   1f, new Color(144 / 255f, 144 / 255f, 144 / 255f)) },
        {  "N", new Element( "7727-37-9",      "Nitrogen",   7, 1.5f, new Color( 48 / 255f,  80 / 255f, 248 / 255f)) },
        {  "O", new Element( "7782-44-7",        "Oxygen",   8, 1.5f, new Color(255 / 255f,  13 / 255f,  13 / 255f)) },
        {  "F", new Element( "7782-41-4",      "Fluorine",   9, 1.5f, new Color(144 / 255f, 224 / 255f,  80 / 255f)) },
        { "Ne", new Element( "7440-01-9",          "Neon",  10, 1.5f, new Color(179 / 255f, 227 / 255f, 245 / 255f)) },
        { "Na", new Element( "7440-23-5",        "Sodium",  11, 1.5f, new Color(171 / 255f,  92 / 255f, 242 / 255f)) },
        { "Mg", new Element( "7439-95-4",     "Magnesium",  12, 1.5f, new Color(138 / 255f, 255 / 255f,   0 / 255f)) },
        { "Al", new Element( "7429-90-5",      "Aluminum",  13, 1.5f, new Color(191 / 255f, 166 / 255f, 166 / 255f)) },
        { "Si", new Element( "7440-21-3",       "Silicon",  14, 1.5f, new Color(240 / 255f, 200 / 255f, 160 / 255f)) },
        {  "P", new Element( "7723-14-0",    "Phosphorus",  15, 1.5f, new Color(255 / 255f, 128 / 255f,   0 / 255f)) },
        {  "S", new Element( "7704-34-9",        "Sulfur",  16, 1.5f, new Color(255 / 255f, 255 / 255f,  48 / 255f)) },
        { "Cl", new Element( "7782-50-5",      "Chlorine",  17, 1.5f, new Color( 31 / 255f, 240 / 255f,  31 / 255f)) },
        { "Ar", new Element( "7440-37-1",         "Argon",  18, 1.5f, new Color(128 / 255f, 209 / 255f, 227 / 255f)) },
        {  "K", new Element( "7440-09-7",     "Potassium",  19, 1.5f, new Color(143 / 255f,  64 / 255f, 212 / 255f)) },
        { "Ca", new Element( "7440-70-2",       "Calcium",  20, 1.5f, new Color( 61 / 255f, 255 / 255f,   0 / 255f)) },
        { "Sc", new Element( "7440-20-2",      "Scandium",  21, 1.5f, new Color(230 / 255f, 230 / 255f, 230 / 255f)) },
        { "Ti", new Element( "7440-32-6",      "Titanium",  22, 1.5f, new Color(191 / 255f, 194 / 255f, 199 / 255f)) },
        {  "V", new Element( "7440-62-2",      "Vanadium",  23, 1.5f, new Color(166 / 255f, 166 / 255f, 171 / 255f)) },
        { "Cr", new Element( "7440-47-3",      "Chromium",  24, 1.5f, new Color(138 / 255f, 153 / 255f, 199 / 255f)) },
        { "Mn", new Element( "7439-96-5",     "Manganese",  25, 1.5f, new Color(156 / 255f, 122 / 255f, 199 / 255f)) },
        { "Fe", new Element( "7439-89-6",          "Iron",  26, 1.5f, new Color(224 / 255f, 102 / 255f,  51 / 255f)) },
        { "Co", new Element( "7440-48-4",        "Cobalt",  27, 1.5f, new Color(240 / 255f, 144 / 255f, 160 / 255f)) },
        { "Ni", new Element( "7440-02-0",        "Nickel",  28, 1.5f, new Color( 80 / 255f, 208 / 255f,  80 / 255f)) },
        { "Cu", new Element( "7440-50-8",        "Copper",  29, 1.5f, new Color(200 / 255f, 128 / 255f,  51 / 255f)) },
        { "Zn", new Element( "7440-66-6",          "Zinc",  30, 1.5f, new Color(125 / 255f, 128 / 255f, 176 / 255f)) },
        { "Ga", new Element( "7440-55-3",       "Gallium",  31, 1.5f, new Color(194 / 255f, 143 / 255f, 143 / 255f)) },
        { "Ge", new Element( "7440-56-4",     "Germanium",  32, 1.5f, new Color(102 / 255f, 143 / 255f, 143 / 255f)) },
        { "As", new Element( "7440-38-2",       "Arsenic",  33, 1.5f, new Color(189 / 255f, 128 / 255f, 227 / 255f)) },
        { "Se", new Element( "7782-49-2",      "Selenium",  34, 1.5f, new Color(255 / 255f, 161 / 255f,   0 / 255f)) },
        { "Br", new Element( "7726-95-6",       "Bromine",  35, 1.5f, new Color(166 / 255f,  41 / 255f,  41 / 255f)) },
        { "Kr", new Element( "7439-90-9",       "Krypton",  36, 1.5f, new Color( 92 / 255f, 184 / 255f, 209 / 255f)) },
        { "Rb", new Element( "7440-17-7",      "Rubidium",  37, 1.5f, new Color(112 / 255f,  46 / 255f, 176 / 255f)) },
        { "Sr", new Element( "7440-24-6",     "Strontium",  38, 1.5f, new Color(  0 / 255f, 255 / 255f,   0 / 255f)) },
        {  "Y", new Element( "7440-65-5",       "Yttrium",  39, 1.5f, new Color(148 / 255f, 255 / 255f, 255 / 255f)) },
        { "Zr", new Element( "7440-67-7",     "Zirconium",  40, 1.5f, new Color(148 / 255f, 224 / 255f, 224 / 255f)) },
        { "Nb", new Element( "7440-03-1",       "Niobium",  41, 1.5f, new Color(115 / 255f, 194 / 255f, 201 / 255f)) },
        { "Mo", new Element( "7439-98-7",    "Molybdenum",  42, 1.5f, new Color( 84 / 255f, 181 / 255f, 181 / 255f)) },
        { "Tc", new Element( "7440-26-8",    "Technetium",  43, 1.5f, new Color( 59 / 255f, 158 / 255f, 158 / 255f)) },
        { "Ru", new Element( "7440-18-8",     "Ruthenium",  44, 1.5f, new Color( 36 / 255f, 143 / 255f, 143 / 255f)) },
        { "Rh", new Element( "7440-16-6",       "Rhodium",  45, 1.5f, new Color( 10 / 255f, 125 / 255f, 140 / 255f)) },
        { "Pd", new Element( "7440-05-3",     "Palladium",  46, 1.5f, new Color(  0 / 255f, 105 / 255f, 133 / 255f)) },
        { "Ag", new Element( "7440-22-4",        "Silver",  47, 1.5f, new Color(192 / 255f, 192 / 255f, 192 / 255f)) },
        { "Cd", new Element( "7440-43-9",       "Cadmium",  48, 1.5f, new Color(255 / 255f, 217 / 255f, 143 / 255f)) },
        { "In", new Element( "7440-74-6",        "Indium",  49, 1.5f, new Color(166 / 255f, 117 / 255f, 115 / 255f)) },
        { "Sn", new Element( "7440-31-5",           "Tin",  50, 1.5f, new Color(102 / 255f, 128 / 255f, 128 / 255f)) },
        { "Sb", new Element( "7440-36-0",      "Antimony",  51, 1.5f, new Color(158 / 255f,  99 / 255f, 181 / 255f)) },
        { "Te", new Element("13494-80-9",     "Tellurium",  52, 1.5f, new Color(212 / 255f, 122 / 255f,   0 / 255f)) },
        {  "I", new Element( "7553-56-2",        "Iodine",  53, 1.5f, new Color(148 / 255f,   0 / 255f, 148 / 255f)) },
        { "Xe", new Element( "7440-63-3",         "Xenon",  54, 1.5f, new Color( 66 / 255f, 158 / 255f, 176 / 255f)) },
        { "Cs", new Element( "7440-46-2",        "Cesium",  55, 1.5f, new Color( 87 / 255f,  23 / 255f, 143 / 255f)) },
        { "Ba", new Element( "7440-39-3",        "Barium",  56, 1.5f, new Color(  0 / 255f, 201 / 255f,   0 / 255f)) },
        { "La", new Element( "7439-91-0",     "Lanthanum",  57, 1.5f, new Color(112 / 255f, 212 / 255f, 255 / 255f)) },
        { "Ce", new Element( "7440-45-1",        "Cerium",  58, 1.5f, new Color(255 / 255f, 255 / 255f, 199 / 255f)) },
        { "Pr", new Element( "7440-10-0",  "Praseodymium",  59, 1.5f, new Color(217 / 255f, 255 / 255f, 199 / 255f)) },
        { "Nd", new Element( "7440-00-8",     "Neodymium",  60, 1.5f, new Color(199 / 255f, 255 / 255f, 199 / 255f)) },
        { "Pm", new Element( "7440-12-2",    "Promethium",  61, 1.5f, new Color(163 / 255f, 255 / 255f, 199 / 255f)) },
        { "Sm", new Element( "7440-19-9",      "Samarium",  62, 1.5f, new Color(143 / 255f, 255 / 255f, 199 / 255f)) },
        { "Eu", new Element( "7440-53-1",      "Europium",  63, 1.5f, new Color( 97 / 255f, 255 / 255f, 199 / 255f)) },
        { "Gd", new Element( "7440-54-2",    "Gadolinium",  64, 1.5f, new Color( 69 / 255f, 255 / 255f, 199 / 255f)) },
        { "Tb", new Element( "7440-27-9",       "Terbium",  65, 1.5f, new Color( 48 / 255f, 255 / 255f, 199 / 255f)) },
        { "Dy", new Element( "7429-91-6",    "Dysprosium",  66, 1.5f, new Color( 31 / 255f, 255 / 255f, 199 / 255f)) },
        { "Ho", new Element( "7440-60-0",       "Holmium",  67, 1.5f, new Color(  0 / 255f, 255 / 255f, 156 / 255f)) },
        { "Er", new Element( "7440-52-0",        "Erbium",  68, 1.5f, new Color(  0 / 255f, 230 / 255f, 117 / 255f)) },
        { "Tm", new Element( "7440-30-4",       "Thulium",  69, 1.5f, new Color(  0 / 255f, 212 / 255f,  82 / 255f)) },
        { "Yb", new Element( "7440-64-4",     "Ytterbium",  70, 1.5f, new Color(  0 / 255f, 191 / 255f,  56 / 255f)) },
        { "Lu", new Element( "7439-94-3",      "Lutetium",  71, 1.5f, new Color(  0 / 255f, 171 / 255f,  36 / 255f)) },
        { "Hf", new Element( "7440-58-6",       "Hafnium",  72, 1.5f, new Color( 77 / 255f, 194 / 255f, 255 / 255f)) },
        { "Ta", new Element( "7440-25-7",      "Tantalum",  73, 1.5f, new Color( 77 / 255f, 166 / 255f, 255 / 255f)) },
        {  "W", new Element( "7440-33-7",      "Tungsten",  74, 1.5f, new Color( 33 / 255f, 148 / 255f, 214 / 255f)) },
        { "Re", new Element( "7440-15-5",       "Rhenium",  75, 1.5f, new Color( 38 / 255f, 125 / 255f, 171 / 255f)) },
        { "Os", new Element( "7440-04-2",        "Osmium",  76, 1.5f, new Color( 38 / 255f, 102 / 255f, 150 / 255f)) },
        { "Ir", new Element( "7439-88-5",       "Iridium",  77, 1.5f, new Color( 23 / 255f,  84 / 255f, 135 / 255f)) },
        { "Pt", new Element( "7440-06-4",      "Platinum",  78, 1.5f, new Color(208 / 255f, 208 / 255f, 224 / 255f)) },
        { "Au", new Element( "7440-57-5",          "Gold",  79, 1.5f, new Color(255 / 255f, 209 / 255f,  35 / 255f)) },
        { "Hg", new Element( "7439-97-6",       "Mercury",  80, 1.5f, new Color(184 / 255f, 184 / 255f, 208 / 255f)) },
        { "Tl", new Element( "7440-28-0",      "Thallium",  81, 1.5f, new Color(166 / 255f,  84 / 255f,  77 / 255f)) },
        { "Pb", new Element( "7439-92-1",          "Lead",  82, 1.5f, new Color( 87 / 255f,  89 / 255f,  97 / 255f)) },
        { "Bi", new Element( "7440-69-9",       "Bismuth",  83, 1.5f, new Color(158 / 255f,  79 / 255f, 181 / 255f)) },
        { "Po", new Element( "7440-08-6",      "Polonium",  84, 1.5f, new Color(171 / 255f,  92 / 255f,   0 / 255f)) },
        { "At", new Element( "7440-68-8",      "Astatine",  85, 1.5f, new Color(117 / 255f,  79 / 255f,  69 / 255f)) },
        { "Rn", new Element("10043-92-2",         "Radon",  86, 1.5f, new Color( 66 / 255f, 130 / 255f, 150 / 255f)) },
        { "Fr", new Element( "7440-73-5",      "Francium",  87, 1.5f, new Color( 66 / 255f,   0 / 255f, 102 / 255f)) },
        { "Ra", new Element( "7440-14-4",        "Radium",  88, 1.5f, new Color(  0 / 255f, 125 / 255f,   0 / 255f)) },
        { "Ac", new Element( "7440-34-8",      "Actinium",  89, 1.5f, new Color(112 / 255f, 171 / 255f, 250 / 255f)) },
        { "Th", new Element( "7440-29-1",       "Thorium",  90, 1.5f, new Color(  0 / 255f, 186 / 255f, 255 / 255f)) },
        { "Pa", new Element( "7440-13-3",  "Protactinium",  91, 1.5f, new Color(  0 / 255f, 161 / 255f, 255 / 255f)) },
        {  "U", new Element( "7440-61-1",       "Uranium",  92, 1.5f, new Color(  0 / 255f, 143 / 255f, 255 / 255f)) },
        { "Np", new Element( "7439-99-8",     "Neptunium",  93, 1.5f, new Color(  0 / 255f, 128 / 255f, 255 / 255f)) },
        { "Pu", new Element( "7440-07-5",     "Plutonium",  94, 1.5f, new Color(  0 / 255f, 107 / 255f, 255 / 255f)) },
        { "Am", new Element( "7440-35-9",     "Americium",  95, 1.5f, new Color( 84 / 255f,  92 / 255f, 242 / 255f)) },
        { "Cm", new Element( "7440-51-9",        "Curium",  96, 1.5f, new Color(120 / 255f,  92 / 255f, 227 / 255f)) },
        { "Bk", new Element( "7440-40-6",     "Berkelium",  97, 1.5f, new Color(138 / 255f,  79 / 255f, 227 / 255f)) },
        { "Cf", new Element( "7440-71-3",   "Californium",  98, 1.5f, new Color(161 / 255f,  54 / 255f, 212 / 255f)) },
        { "Es", new Element( "7429-92-7",   "Einsteinium",  99, 1.5f, new Color(179 / 255f,  31 / 255f, 212 / 255f)) },
        { "Fm", new Element( "7440-72-4",       "Fermium", 100, 1.5f, new Color(179 / 255f,  31 / 255f, 186 / 255f)) },
        { "Md", new Element( "7440-11-1",   "Mendelevium", 101, 1.5f, new Color(179 / 255f,  13 / 255f, 166 / 255f)) },
        { "No", new Element("10028-14-5",      "Nobelium", 102, 1.5f, new Color(189 / 255f,  13 / 255f, 135 / 255f)) },
        { "Lr", new Element("22537-19-5",    "Lawrencium", 103, 1.5f, new Color(199 / 255f,   0 / 255f, 102 / 255f)) },
        { "Rf", new Element("53850-36-5", "Rutherfordium", 104, 1.5f, new Color(204 / 255f,   0 / 255f,  89 / 255f)) },
        { "Db", new Element("53850-35-4",       "Dubnium", 105, 1.5f, new Color(209 / 255f,   0 / 255f,  79 / 255f)) },
        { "Sg", new Element("54038-81-2",    "Seaborgium", 106, 1.5f, new Color(217 / 255f,   0 / 255f,  69 / 255f)) },
        { "Bh", new Element("54037-14-8",       "Bohrium", 107, 1.5f, new Color(224 / 255f,   0 / 255f,  56 / 255f)) },
        { "Hs", new Element("54037-57-9",       "Hassium", 108, 1.5f, new Color(230 / 255f,   0 / 255f,  46 / 255f)) },
        { "Mt", new Element("54038-01-6",    "Meitnerium", 109, 1.5f, new Color(235 / 255f,   0 / 255f,  38 / 255f)) }
    };

    // the following functions allow to access the properties of the elements
    public static string GetCasNumber(string shortName) {
        return m_localElementDict[shortName].m_casNumber;
    }

    public static string GetFullName(string shortName) {
        return m_localElementDict[shortName].m_fullName;
    }

    public static int GetOrdinalNumber(string shortName) {
        return m_localElementDict[shortName].m_ordinalNumber;
    }

    public static float GetSize(string shortName) {
        return m_localElementDict[shortName].m_size;
    }

    public static Color GetColour(string shortName) {
        return m_localElementDict[shortName].m_colour;
    }
}
