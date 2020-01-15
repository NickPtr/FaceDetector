using System;
using System.Collections.Generic;
using System.ComponentModel;
using Xamarin.Forms;
using Plugin.Media;
using XamarinCognitive.Models;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net.Http.Headers;
using System.IO;

namespace FaceDetection
{
    [DesignTimeVisible(false)]
    public partial class MainPage : ContentPage
    {
        public string subscriptionKey = "acf2940406154ca9b78f67410a500b75";//Dilosi tou kleidiou gia tin xrisi tou API

        public string uriBase = "https://nikos.cognitiveservices.azure.com/";//Dilosi tou endpoint gia tin xrisi tou API


        public MainPage()
        {
            InitializeComponent();//Dimiourgia grafikwn
        }

        //Dilwsi leitourgiwn gia to koumpi
        async void btnPick_Clicked(object sender, System.EventArgs e)
        {
            //leitourgies gia tin epilogi fotografias apo to album tou kinitou
            await CrossMedia.Current.Initialize();
            try
            {
                var file = await CrossMedia.Current.PickPhotoAsync(new Plugin.Media.Abstractions.PickMediaOptions
                {
                    PhotoSize = Plugin.Media.Abstractions.PhotoSize.Medium
                });
                if (file == null) return;
                imgSelected.Source = ImageSource.FromStream(() =>
                {
                    var stream = file.GetStream();
                    return stream;
                });
                MakeAnalysisRequest(file.Path);//kaloume tin sinartisi gia tin simdesi me to API
            }
            catch (Exception ex)
            {
            }
        }

        //Dilwsi leitourgiwn gia to koumpi
        async void CameraButton_Clicked(object sender, EventArgs e)
        {
            //leitourgies gia tin epilogi fotografias apo tin kamera tou kinitou
            var photo = await CrossMedia.Current.TakePhotoAsync(new Plugin.Media.Abstractions.StoreCameraMediaOptions()
            {
                PhotoSize = Plugin.Media.Abstractions.PhotoSize.Medium
            });

            if (photo != null)
                imgSelected.Source = ImageSource.FromStream(() =>
                {

                    var camerasStream = photo.GetStream();
                    return camerasStream;
                });
            else
            {
                
            }
            MakeAnalysisRequest(photo.Path);//kaloume tin sinartisi gia tin simdesi me to API
        }

        //Dimiourgia tis sinartisis gia tin sindesi me to API
        public async void MakeAnalysisRequest(string imageFilePath)
        {
            //arxikopoihsi tis sindesis gia to etima
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", subscriptionKey);
            //arxikopoihsi twn stoixiwn pou zitame
            string requestParameters = "returnFaceId=true&returnFaceLandmarks=false" +
                "&returnFaceAttributes=age,gender,headPose,smile,facialHair,glasses," +
                "emotion,hair,makeup,occlusion,accessories,blur,exposure,noise";

            string uri = uriBase + "face/v1.0/detect?" + requestParameters;//dimiourgia tou url gia to etima
            HttpResponseMessage response;//arxikopoihsi tis metavlitis gia tin apantisi
            byte[] byteData = GetImageAsByteArray(imageFilePath);//kaloume tin sinartisi gia tin metatropi tis fotografias se bytes

            using (ByteArrayContent content = new ByteArrayContent(byteData))
            {
                content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");//dilosi tou tupou tou reques pou kanoume
                response = await client.PostAsync(uri, content);//pernoume tin apantisi tou etimatos 

                string contentString = await response.Content.ReadAsStringAsync();//metatropi tis apantisis se string
                //Dimiourgiias listas tupou ResponeModel kai kanoume Deserialize to string tis apantisis
                List<ResponseModel> faceDetails = JsonConvert.DeserializeObject<List<ResponseModel>>(contentString);
                //Elenxos gia to an exoume parei swsti apantisi 
                if (faceDetails.Count != 0)
                {
                    //amfanisi twn apotelesmatwn sta katalila Labels
                    lblTotalFace.Text = "Total Faces : " + faceDetails.Count;
                    //elenxos gia to an exoume ena h duo prosopa stin fotagrafia gia tin swsti emfanisi twn apotelesmatwn
                    if(faceDetails.Count==2)
                    {
                        lblGender.Text = "Gender : " + faceDetails[0].faceAttributes.gender + ", " + faceDetails[1].faceAttributes.gender;
                        lblAge.Text = "Estimated Age : " + faceDetails[0].faceAttributes.age + ", " + faceDetails[1].faceAttributes.age;
                        lblHair.Text = "Bald : " + faceDetails[0].faceAttributes.hair.bald * 100 + "%" + ", " + faceDetails[1].faceAttributes.hair.bald * 100 + "%";
                        lblBeard.Text = "Beard : " + faceDetails[0].faceAttributes.facialHair.beard * 100 + "%" + ", " + faceDetails[1].faceAttributes.facialHair.beard * 100 + "%";
                        lblGlasses.Text = "Glasses : " + faceDetails[0].faceAttributes.glasses + ", " + faceDetails[1].faceAttributes.glasses;
                        if (faceDetails[0].faceAttributes.makeup.eyeMakeup)
                        {
                            if(faceDetails[1].faceAttributes.makeup.eyeMakeup)
                                lblEyeMakeup.Text = "EyeMakeup : Yes, Yes";
                            else
                                lblEyeMakeup.Text = "EyeMakeup : Yes, No";
                        }
                        else
                        {
                            if (faceDetails[1].faceAttributes.makeup.eyeMakeup)
                                lblEyeMakeup.Text = "EyeMakeup : No, Yes";
                            else
                                lblEyeMakeup.Text = "EyeMakeup : No, No";
                        }
                            
                        if (faceDetails[0].faceAttributes.makeup.lipMakeup)
                        {
                            if (faceDetails[1].faceAttributes.makeup.lipMakeup)
                                lblLipMakeup.Text = "LipMakeup : Yes, Yes";
                            else
                                lblLipMakeup.Text = "EyeMakeup : Yes, No";
                        }
                        else
                        {
                            if (faceDetails[1].faceAttributes.makeup.lipMakeup)
                                lblLipMakeup.Text = "LipMakeup : No, Yes";
                            else
                                lblLipMakeup.Text = "EyeMakeup : No, No";
                        }
                            
                        if (faceDetails[0].faceAttributes.emotion.anger != 0 || faceDetails[1].faceAttributes.emotion.anger != 0)
                        {
                            lblAnger.Text = "Emotion : Anger " + faceDetails[0].faceAttributes.emotion.anger * 100 + "%" + ", " + faceDetails[1].faceAttributes.emotion.anger * 100 + "%";
                        }
                        else
                            lblAnger.Text = "";
                        if (faceDetails[0].faceAttributes.emotion.happiness != 0 || faceDetails[1].faceAttributes.emotion.happiness != 0)
                        {
                            lblHappy.Text = "Emotion : Happiness " + faceDetails[0].faceAttributes.emotion.happiness * 100 + "%" + ", " + faceDetails[1].faceAttributes.emotion.happiness * 100 + "%";
                        }
                        else
                            lblHappy.Text = "";
                        if (faceDetails[0].faceAttributes.emotion.fear != 0 || faceDetails[1].faceAttributes.emotion.fear != 0)
                        {
                            lblFear.Text = "Emotion : Fear " + faceDetails[0].faceAttributes.emotion.fear * 100 + "%" + ", " + faceDetails[1].faceAttributes.emotion.fear * 100 + "%";
                        }
                        else
                            lblFear.Text = "";
                    }
                    else if (faceDetails.Count==1)
                    {
                        lblGender.Text = "Gender : " + faceDetails[0].faceAttributes.gender;
                        lblAge.Text = "Estimated Age : " + faceDetails[0].faceAttributes.age;
                        lblHair.Text = "Bald : " + faceDetails[0].faceAttributes.hair.bald * 100 + "%";
                        lblBeard.Text = "Beard : " + faceDetails[0].faceAttributes.facialHair.beard * 100 + "%";
                        lblGlasses.Text = "Glasses : " + faceDetails[0].faceAttributes.glasses;
                        if (faceDetails[0].faceAttributes.makeup.eyeMakeup)
                            lblEyeMakeup.Text = "EyeMakeup : Yes";
                        else
                            lblEyeMakeup.Text = "EyeMakeup : No";
                        if (faceDetails[0].faceAttributes.makeup.lipMakeup)
                            lblLipMakeup.Text = "LipMakeup : Yes";
                        else
                            lblLipMakeup.Text = "LipMakeup : No";
                        if (faceDetails[0].faceAttributes.emotion.anger != 0)
                        {
                            lblAnger.Text = "Emotion : Anger " + faceDetails[0].faceAttributes.emotion.anger * 100 + "%";
                        }
                        else
                            lblAnger.Text = "";
                        if (faceDetails[0].faceAttributes.emotion.happiness != 0)
                        {
                            lblHappy.Text = "Emotion : Happiness " + faceDetails[0].faceAttributes.emotion.happiness * 100 + "%";
                        }
                        else
                            lblHappy.Text = "";
                        if (faceDetails[0].faceAttributes.emotion.fear != 0)
                        {
                            lblFear.Text = "Emotion : Fear " + faceDetails[0].faceAttributes.emotion.fear * 100 + "%";
                        }
                        else
                            lblFear.Text = "";
                    }
                    

                }

            }
        }

        //Dimiourgia sinartisis gia tin metatropi tis fotografias se bytes
        public byte[] GetImageAsByteArray(string imageFilePath)
        {
            using (FileStream fileStream =
                new FileStream(imageFilePath, FileMode.Open, FileAccess.Read))
            {
                BinaryReader binaryReader = new BinaryReader(fileStream);
                return binaryReader.ReadBytes((int)fileStream.Length);
            }
        }
    }
}
