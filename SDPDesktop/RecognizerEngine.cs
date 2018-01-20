using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Face;
using Emgu.CV.Structure;

namespace SDPDesktop
{
    public class RecognizerEngine
    {
        private readonly FaceRecognizer _faceRecognizer;
        private readonly string _recognizerFilePath;

        public RecognizerEngine(string recognizerFilePath)
        {
            _recognizerFilePath = recognizerFilePath;
            //_faceRecognizer = new EigenFaceRecognizer(80, double.PositiveInfinity);
            _faceRecognizer = new LBPHFaceRecognizer(1, 8, 8, 50); //also try to use 100 threshold for less strict recognition.
        }

        /// <summary>
        /// Method for training the images from the database.
        /// </summary>
        public void TrainRecognizer()
        {
            try
            {
                using (var context = new FRModel())
                {
                    var allFaces = context.Images.ToList();
                    var faceImages = new Image<Gray, byte>[allFaces.Count];
                    var faceLabels = new int[allFaces.Count];
                    for (var i = 0; i < allFaces.Count; i++)
                    {
                        Stream stream = new MemoryStream();
                        stream.Write(allFaces[i].Face, 0, allFaces[i].Face.Length);
                        var faceImage = new Image<Gray, byte>(new Bitmap(stream));
                        faceImages[i] = faceImage.Resize(100, 100, Inter.Cubic);
                        faceLabels[i] = allFaces[i].UserId;
                    }
                    _faceRecognizer.Train(faceImages, faceLabels);
                    _faceRecognizer.Save(_recognizerFilePath);
                }
            }
            catch (SqlException)
            {
                MessageBox.Show(@"Error occured while trying to train the data for facial recognition.",
                    @"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Method which recognizes the user from the given image.
        /// </summary>
        /// <param name="userImage"></param>
        /// <returns></returns>
        public FaceRecognizer.PredictionResult RecognizeUser(Image<Gray, byte> userImage)
        {
            _faceRecognizer.Load(_recognizerFilePath);

            ////normalize brightness
            //userImage._EqualizeHist();

            var result = _faceRecognizer.Predict(userImage.Resize(100, 100, Inter.Cubic));
            return result;
        }
    }
}
