using CG.Web.MegaApiClient;
using Microsoft.VisualBasic.FileIO;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

readFromFile();

void readFromFile()
{
    int countLine = File.ReadAllLines("words.txt").Count();
    string[] keyWords = new string[countLine];

    using (StreamReader reader = new StreamReader("words.txt"))
        for (int i = 0; i < countLine; i++) keyWords[i] = reader.ReadLine();

    foreach (string keyWord in keyWords) findPhrases(keyWord);
}

void findPhrases(string keyWord)
{
    ChromeOptions options = new ChromeOptions();
    options.AddArgument("--headless");

    IWebDriver driver = new ChromeDriver(options);
    string baseURL = "https://www.bukvarix.com";
    driver.Url = baseURL;

    driver.FindElement(By.Id("SearchFormIndexQ")).SendKeys($"{keyWord}" + Keys.Enter);

    string downloadLink = driver
                .FindElement(By
                .CssSelector(".download-block a.report-download-button"))
                .GetAttribute("href");

    driver.Dispose();
    downloadFile(downloadLink);
}

void downloadFile(string downloadLink)
{
    using (var client = new HttpClient())
    using (var s = client.GetStreamAsync(downloadLink))
    using (var fs = new FileStream("файл.csv", FileMode.Create))
        s.Result.CopyTo(fs);

    uploadCSV();
    parseCSV();
}

void uploadCSV()
{
    MegaApiClient client = new MegaApiClient();
    client.Login("email", "password");

    IEnumerable<INode> nodes = client.GetNodes(); 
    INode root = nodes.Single(x => x.Type == NodeType.Root); 

    INode myFile = client.UploadFile("файл.csv", root);
    Uri downloadLink = client.GetDownloadLink(myFile); 
}

void parseCSV()
{
    using (TextFieldParser parser = new TextFieldParser("файл.csv"))
    {
        parser.TextFieldType = FieldType.Delimited;
        parser.SetDelimiters(";");

        //Read header row
        if (!parser.EndOfData) parser.ReadLine();

        while (!parser.EndOfData)
        {
            //Process row
            string[] fields = parser.ReadFields();
            checkDuplicate(fields[0]);
        }
    }
}

void checkDuplicate(string str)
{
    using (StreamReader reader = new StreamReader("words.txt"))
    {
        string[] words = str.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        string? line;
        while ((line = reader.ReadLine()) != null)
            foreach (string word in words)
                if (line.Contains(word)) str = str.Replace(word, "").Trim();
    }

    if (str != string.Empty) writePhrase(str);
}

void writePhrase(string str)
{
    using (StreamWriter writer = new StreamWriter("words.txt", true))
        writer.WriteLine(str);
}