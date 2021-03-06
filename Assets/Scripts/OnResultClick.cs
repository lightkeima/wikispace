﻿using AngleSharp.Html.Parser;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using AngleSharp;
using System.Linq;
using AngleSharp.Dom;
using TMPro;
using System.Threading;
using UnityEngine.Networking;
using Newtonsoft.Json;
using Coffee.UIEffects;

public class OnResultClick : MonoBehaviour
{
    const string H2 = "<h2></h2>";
    const string H3 = "<h3></h3>";
    const string H4 = "<h4></h4>";
    const string H5 = "<h5></h5>";
    static string html;
    static public string pretitle = "";
    public Text title;
    public static string currenttitle = "";
    static public Dictionary<string, List<GameObject>> dicpanels = new Dictionary<string, List<GameObject>>();
    private List<GameObject> imagepanels = new List<GameObject>();
    public Button btn;


    const string APIUrl = "https://en.wikipedia.org/w/api.php";


    const string ParseAction = "?action=query&format=json&prop=extracts&formatversion=2&titles=";

    const string GetImage = "?action=query&format=json&prop=images&formatversion=2&titles=";

    const string GetImageFromUrl = "https://commons.wikimedia.org/wiki/Special:FilePath/";


    IEnumerator ShowPage()
    {
        if (currenttitle == pretitle) ;
        else
        using (UnityWebRequest webRequest = UnityWebRequest.Get(APIUrl + ParseAction + currenttitle))
        {
            // Request and wait for the desired page.
            yield return webRequest.SendWebRequest();

            if (webRequest.isNetworkError)
            {
            }
                else
            {
                    // Show results as text
                string json = (webRequest.downloadHandler.text);
                var values = JsonConvert.DeserializeObject<PageContent>(json);
                html = values.query["pages"][0]["extract"];
                ModelManager.ShowModel(currenttitle);
                    StartCoroutine(HTMLParse());
                    StartCoroutine(GetListImage());

                    // Or retrieve results as binary data
               }
            }
    }
    IEnumerator GetListImage()
    {
        string replacedtitle = currenttitle.Replace(" ", "_");
        using (UnityWebRequest webRequest = UnityWebRequest.Get(APIUrl + GetImage + replacedtitle   ))
        {
            // Request and wait for the desired page.
            yield return webRequest.SendWebRequest();

            if (webRequest.isNetworkError)
            {
            }
            else
            {
                // Show results as text
                string json = (webRequest.downloadHandler.text);
                //TODO: find sub set if it contains "images" then 
                if (json.Contains("images"))
                {
                    var values = JsonConvert.DeserializeObject<ImageList>(json);
                    List<GameObject> panels = new List<GameObject>();
                    foreach (GameObject ip in imagepanels) {
                        GameObject.Destroy(ip);
                    }
                    imagepanels.Clear();
                    html = values.query.pages[0].images[0]["title"];
                    int i = 0;
                    foreach (Dictionary<string, string> item in values.query.pages[0].images) {
                       
                        string title = item["title"].Replace("File:","");
                        if (!title.Contains("svg"))
                        {
                            imagepanels.Add(Instantiate(Resources.Load("Prefabs/ImageBlock", typeof(GameObject)), new Vector3(-14 + i * 12  , 0, -24), new Quaternion(0,180f,0,0)) as GameObject);
                            StartCoroutine(ShowImage(GetImageFromUrl + title, i));
                            ++i;
                            yield return new WaitForSeconds(1f);
                        }
                    }

                }
            }
        }
    }
    IEnumerator ShowImage(string url, int i)
    {
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);

        yield return request.SendWebRequest();

        if (request.isNetworkError || request.isHttpError) ;
        else
        {
            int _width = ((DownloadHandlerTexture)request.downloadHandler).texture.width;
            int _height = ((DownloadHandlerTexture)request.downloadHandler).texture.height;
            int width = 600 * _width / _height;
            int height = 600 * _height / _width;
            imagepanels[i].GetComponent<RectTransform>().sizeDelta = new Vector2(width, height);
            imagepanels[i].GetComponent<UIDissolve>().Play();
            imagepanels[i].GetComponent<RawImage>().texture = ((DownloadHandlerTexture)request.downloadHandler).texture;
        }

    }
    public void OnButtonClick() {
        currenttitle = title.text;
        SetActiveAll(pretitle, false);
        if (dicpanels.ContainsKey(currenttitle))
        {
            SetActiveAll(currenttitle, true);
            return;
        }
        else
        {
            StartCoroutine(ShowPage());
        }
    }

    void Awake()
    {
        btn.onClick.AddListener(OnButtonClick);
    }
    static int isHeader(string text) {
        switch (text) {
            case H2: return 1;
            case H3: return 2;
            case H4: return 3;
            case H5: return 4;
        }
        return 0;
    }
    static public void SetActiveAll(string title, bool isactive)
    {

        if(dicpanels.ContainsKey(title)){
            for(int i = 0; i < dicpanels[title].Count;++i){
                dicpanels[title][i].SetActive(isactive);
            }
        }
    }



    public IEnumerator HTMLParse() {
        List<GameObject>  panels = new List<GameObject>();
        var config = Configuration.Default;

        var context = BrowsingContext.New(config);
        
        var parser = context.GetService<IHtmlParser>();

        var document = parser.ParseDocument(html);

        var header = document.All.Where(m => m.LocalName == "h2" || m.LocalName == "h3" || m.LocalName == "h4" || m.LocalName == "h5");
        
        List<string> headers= new List<string>();
        foreach (var item in header)
        {
            headers.Add(item.Text());
        }

        var test = document.All.Where(m => m.LocalName != "b" && m.LocalName != "i" && m.LocalName != "a");
       
        foreach (var element in document.QuerySelectorAll("span"))
        {
            element.Remove();
        }
        int currentContent = 0;
        int previousSection = 0;
        //int previousSubSection = 0;
        //int previousSubSubsection = 0;
        panels.Add(Instantiate(Resources.Load("Prefabs/Content", typeof(GameObject)), new Vector3(-2, -3.5f, 0), Quaternion.identity) as GameObject);
        panels[0].transform.GetChild(0).GetChild(0).gameObject.GetComponent<Text>().text = currenttitle;
        panels[0].transform.GetChild(0).GetComponent<UIDissolve>().Play();
        panels[0].transform.GetChild(1).GetComponent<UIDissolve>().Play();
        panels[0].transform.GetChild(2).gameObject.SetActive(false);
        panels[0].transform.GetChild(3).gameObject.SetActive(false);
        panels[0].transform.GetChild(4).gameObject.SetActive(false);
        panels[0].transform.GetChild(5).gameObject.SetActive(false);
        string textcontent = "";
        int sortinglayer = 0;
        foreach (var item in test)
        {
            int type = isHeader(item.OuterHtml);
            if (type != 0) {
                if (type == 1)
                {
                    // move them out if want to move back
                    panels[sortinglayer].transform.GetChild(1).GetChild(0).GetChild(0).GetChild(0).GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().text = textcontent.Trim();
                    textcontent = "";
                    yield return new WaitForSeconds(1f);
                    panels.Add(Instantiate(Resources.Load("Prefabs/Content", typeof(GameObject)), new Vector3(-2 - 8 * (1 + sortinglayer), -3.5f, 0), Quaternion.identity) as GameObject);
                    
                    Canvas p = panels[sortinglayer + 1].transform.GetChild(1).gameObject.GetComponent<Canvas>();
                    panels[sortinglayer + 1].transform.GetChild(1).gameObject.GetComponent<UIDissolve>().Play();
                    p.sortingOrder = -(sortinglayer);
                    //



                    TextMeshProUGUI section = panels[sortinglayer + 1].transform.GetChild(2).GetChild(0).gameObject.GetComponent<TextMeshProUGUI>();
                    section.text = headers[currentContent];
                    panels[sortinglayer + 1].transform.GetChild(0).gameObject.SetActive(false);
                    //panels[currentContent + 1].transform.GetChild(3).gameObject.SetActive(false);
                    //panels[currentContent + 1].transform.GetChild(4).gameObject.SetActive(false);
                    //panels[currentContent + 1].transform.GetChild(5).gameObject.SetActive(false);
                    previousSection = currentContent;
                    Canvas p1 = panels[sortinglayer + 1].transform.GetChild(2).gameObject.GetComponent<Canvas>();
                    panels[sortinglayer + 1].transform.GetChild(2).GetComponent<UIDissolve>().Play();
                    p1.sortingOrder = -(sortinglayer);
                    ++sortinglayer;
                }
                else if (type == 2)
                {
                    /*
                    TextMeshProUGUI presection = panels[currentContent + 1].transform.GetChild(2).GetChild(0).gameObject.GetComponent<TextMeshProUGUI>();
                    TextMeshProUGUI section = panels[currentContent + 1].transform.GetChild(3).GetChild(0).gameObject.GetComponent<TextMeshProUGUI>();
                    section.text = headers[currentContent];
                    presection.text = headers[previousSection];
                    previousSubSection = currentContent;
                    panels[currentContent + 1].transform.GetChild(0).gameObject.SetActive(false);
                    panels[currentContent + 1].transform.GetChild(4).gameObject.SetActive(false);
                    panels[currentContent + 1].transform.GetChild(5).gameObject.SetActive(false);

                    Canvas p1 = panels[currentContent + 1].transform.GetChild(2).gameObject.GetComponent<Canvas>();
                    Canvas p2 = panels[currentContent + 1].transform.GetChild(3).gameObject.GetComponent<Canvas>();
                    p1.sortingOrder = -(currentContent + 1);
                    p2.sortingOrder = -(currentContent + 1);
                    */
                    textcontent += "<size=24><b><color=#0000ffff>" + headers[currentContent] + "</color></b></size>\n  ";
                }
                else if (type == 3)
                {
                    /*
                    TextMeshProUGUI presection = panels[currentContent + 1].transform.GetChild(2).GetChild(0).gameObject.GetComponent<TextMeshProUGUI>();
                    TextMeshProUGUI presubsection = panels[currentContent + 1].transform.GetChild(3).GetChild(0).gameObject.GetComponent<TextMeshProUGUI>();
                    TextMeshProUGUI section = panels[currentContent + 1].transform.GetChild(4).GetChild(0).gameObject.GetComponent<TextMeshProUGUI>();
                    section.text = headers[currentContent];
                    presection.text = headers[previousSection];
                    presubsection.text = headers[previousSubSection];
                    previousSubSubsection = currentContent;
                    panels[currentContent + 1].transform.GetChild(0).gameObject.SetActive(false);
                    panels[currentContent + 1].transform.GetChild(5).gameObject.SetActive(false);

                    Canvas p1 = panels[currentContent + 1].transform.GetChild(2).gameObject.GetComponent<Canvas>();
                    Canvas p2 = panels[currentContent + 1].transform.GetChild(3).gameObject.GetComponent<Canvas>();
                    Canvas p3 = panels[currentContent + 1].transform.GetChild(4).gameObject.GetComponent<Canvas>();
                    p1.sortingOrder = -(currentContent + 1);
                    p2.sortingOrder = -(currentContent + 1);
                    p3.sortingOrder = -(currentContent + 1);
                     */
                    textcontent += "<size=20><b><color=#008080ff>" + headers[currentContent] + "</color></b><size=24>\n  ";

                }
                else if (type == 4)
                {
                    textcontent += "<b>" + headers[currentContent] + "</b>\n  ";

                    /*
                    TextMeshProUGUI presection = panels[currentContent + 1].transform.GetChild(2).GetChild(0).gameObject.GetComponent<TextMeshProUGUI>();
                    TextMeshProUGUI presubsection = panels[currentContent + 1].transform.GetChild(3).GetChild(0).gameObject.GetComponent<TextMeshProUGUI>();
                    TextMeshProUGUI presubsubsection = panels[currentContent + 1].transform.GetChild(4).GetChild(0).gameObject.GetComponent<TextMeshProUGUI>();
                    TextMeshProUGUI section = panels[currentContent + 1].transform.GetChild(5).GetChild(0).gameObject.GetComponent<TextMeshProUGUI>();
                    section.text = headers[currentContent];
                    section.text = headers[currentContent];
                    presection.text = headers[previousSection];
                    presubsection.text = headers[previousSubSection];
                    presubsubsection.text = headers[previousSubSubsection];
                    panels[currentContent + 1].transform.GetChild(0).gameObject.SetActive(false);
                   
                    Canvas p1 = panels[currentContent + 1].transform.GetChild(2).gameObject.GetComponent<Canvas>();
                    Canvas p2 = panels[currentContent + 1].transform.GetChild(3).gameObject.GetComponent<Canvas>();
                    Canvas p3 = panels[currentContent + 1].transform.GetChild(4).gameObject.GetComponent<Canvas>();
                    Canvas p4 = panels[currentContent + 1].transform.GetChild(5).gameObject.GetComponent<Canvas>();
                    p1.sortingOrder = -(currentContent + 1);
                    p2.sortingOrder = -(currentContent + 1);
                    p3.sortingOrder = -(currentContent + 1);
                    p4.sortingOrder = -(currentContent + 1);

                    */
                }
                ++currentContent;

            }
            else
            if (item.Text() != "" && !item.OuterHtml.Contains("</html>") && !item.OuterHtml.Contains("</body>") && !item.OuterHtml.Contains("</ul>"))
            {
                if (!textcontent.Contains("</il>"))
                {
                    textcontent += item.Text() + '\n';
                }
                else {
                    textcontent += "-" + item.Text() + '\n';

                }
            }
        }
        panels[sortinglayer].transform.GetChild(1).GetChild(0).GetChild(0).GetChild(0).GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().text = textcontent.Trim();
        pretitle = currenttitle;
        if(!dicpanels.ContainsKey(currenttitle))
            dicpanels.Add(currenttitle, panels);
    }


}
