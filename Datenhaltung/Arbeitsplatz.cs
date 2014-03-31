using System;
using System.Collections.Generic;
using System.Text;

namespace Tool
{
    public class Arbeitsplatz
    {
        protected int nummer;
        protected int anzSchichten = 1;
        protected int anzUeberMin = 0;
        int warteschlangenZeit = 0;
        /// <summary>
        /// benötigte Zeit zur hersetllung von  teil mit der Key nummer
        /// </summary>
        private Dictionary<int, int> werkZeit;
        protected Dictionary<int, int> ruestzeit;
        private int anzRuestung = 0;

        private Dictionary<int, int> naechsterSchritt = new Dictionary<int, int>();// als Dictionary umschreiben

        public int AnzRuestung
        {
            set
            {
                this.anzRuestung = value;
            }
        }

        public Arbeitsplatz(int nr)
        {
            this.nummer = nr;
            this.ruestzeit = new Dictionary<int, int>();
            this.werkZeit = new Dictionary<int, int>();
        }

        public Dictionary<int, int> WerkZeitJeStk
        {
            get
            {
                return this.werkZeit;
            }

        }

        public void AddRuestzeit(int teil, int zeit)
        {
            if (zeit < 0)
            {
                throw new InvalidValueException(zeit.ToString(), "Ruestzeit am Arbeitsplatz " + this.nummer);
            }
            if ((this.ruestzeit.ContainsKey(teil) && this.ruestzeit[teil] == 0) || !this.ruestzeit.ContainsKey(teil))
            {
                this.ruestzeit[teil] = zeit;
                if (!this.werkZeit.ContainsKey(teil))
                {
                    this.werkZeit[teil] = 0;
                }
            }
            if (!this.ruestzeit.ContainsKey(teil) && this.ruestzeit[teil] != 0)
            {
                throw new InvalidValueException(string.Format("Am Arbeitsplatz {0} ist bereits eine Rüstzeit für das Teil {1} hinterlegt", this.nummer, teil));
            }
        }

        public void AddWerkzeit(int teil, int zeit)
        {
            if (zeit < 0)
            {
                throw new InvalidValueException(zeit.ToString(), "Werkzeit am Arbeitsplatz " + this.nummer);
            }
            if(!this.naechsterSchritt.ContainsKey(teil))
            {
                this.naechsterSchritt[teil] =-1;
            }

            if ((this.werkZeit.ContainsKey(teil) && this.werkZeit[teil] == 0) || !this.werkZeit.ContainsKey(teil))
            {
                this.werkZeit[teil] = zeit;
                if (!this.ruestzeit.ContainsKey(teil))
                {
                    this.ruestzeit[teil] = 0;
                }

                if (!(DataContainer.Instance.GetTeil(teil) as ETeil).BenutzteArbeitsplaetze.Contains(this))
                {
                    (DataContainer.Instance.GetTeil(teil) as ETeil).AddArbeitsplatz(this.nummer);
                }
            }
            if (!this.werkZeit.ContainsKey(teil) && this.werkZeit[teil] != 0 && this.werkZeit[teil] != zeit)
            {
                throw new InvalidValueException(string.Format("Am Arbeitsplatz {0} ist bereits eine Werkzeit für das Teil {1} hinterlegt", this.nummer, teil));
            }
        }

        public int BenoetigteZeit
        {
            get
            {
                DataContainer data = DataContainer.Instance;
                int res = 0;
                foreach (KeyValuePair<int, int> kvp in this.werkZeit)
                {
                    res += kvp.Value * (data.GetTeil(kvp.Key)as ETeil).Produktionsmenge;
                }
                res += this.warteschlangenZeit;
                return res;
            }
        }

        /// <summary>
        /// fügt eine neue Menge in die Warteschlange
        /// </summary>
        /// <param name="teilnr">die nummer des herzustellenden Teils.</param>
        /// <param name="menge">The menge in der Warteschlange</param>
        /// <param name="aktuellerPlatz">wenn<c>true</c> dann ist dieser Arbeitsplatz der an dem die Wartschlange liegt</param>
        public void AddWarteschlange(int teilnr, int menge, bool aktuellerPlatz)
        {
            if (aktuellerPlatz)
            {
                (DataContainer.Instance.GetTeil(teilnr) as ETeil).InWartschlange += menge;
            }
			
            if (!this.werkZeit.ContainsKey(teilnr))
                {
                    this.werkZeit[teilnr] = 0;
                }
            this.warteschlangenZeit += menge * this.werkZeit[teilnr];
			
            
            if (!this.naechsterSchritt.ContainsKey(teilnr))
            {
            } else if(this.naechsterSchritt[teilnr] != -1) {
            	DataContainer.Instance.GetArbeitsplatz(this.naechsterSchritt[teilnr]).AddWarteschlange(teilnr, menge, false);
            }
        }

        public double Ruestzeit
        {
            get
            {
                double sum = 0;
                foreach (KeyValuePair<int, int> kvp in this.ruestzeit)
                {
                    sum += kvp.Value;
                }
                return (sum / this.ruestzeit.Count) * this.anzRuestung;
            }
        }


        /// <summary>
        /// Fügt die Ueberstunden in minuten pro Tag ein
        /// falls es eine neue Schicht eröffnet werden muss wird das gemacht falls max Kapa erreicht wird false zurückgegeben
        /// </summary>
        /// <param name="min">Minuten pro Tag.</param>
        /// <returns></returns>
        public bool AddUeberMinute(int min)
        {
            if (this.anzUeberMin + min < 240)
            {
                this.anzUeberMin += min;
                return true;
            }
            else
            {
                if (this.AddnewSchicht())
                {
                    this.anzUeberMin = 0;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Getter für die Anzahl an Ueberminuten
        /// </summary>
        /// <value>The ueber min.</value>
        public int UeberMin
        {
            get
            {
                return this.anzUeberMin;
            }

            set
            {
                this.anzUeberMin = value;
            }
        }


        /// <summary>
        /// fügt eine neue schicht hinzu gibt false zurück falls es nicht mehr möglich ist
        /// </summary>
        /// <returns></returns>
        public bool AddnewSchicht()
        {
            if (this.anzSchichten < 2)
            {
                this.anzSchichten++;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Gibt die Anzahl der Schichten zurück.
        /// </summary>
        /// <value>The schichten.</value>
        public int Schichten
        {
            get
            {
                return this.anzSchichten;
            }

            set
            {
                this.anzSchichten = value;
            }
        }


        /// <summary>
        /// Zur verfuegung stehende zeit je Woche
        /// </summary>
        /// <value>The zu verfuegung stehende zeit.</value>
        public int ZuVerfuegungStehendeZeit
        {
            get
            {
                return this.anzSchichten * 2400 + anzUeberMin * 5;
            }
        }

        /// <summary>
        /// gibt die Liste aller Teile die an diesem Arbeitsplatz hergestellt werden
        /// </summary>
        /// <returns></returns>
        public List<ETeil> HergestelteTeile
        {
            get
            {
                List<ETeil> list = new List<ETeil>();
                DataContainer cont = DataContainer.Instance;
                foreach (KeyValuePair<int, int> res in this.werkZeit)
                {
                    list.Add(cont.GetTeil(res.Key) as ETeil);
                }
                return list;

            }
        }

        /// <summary>
        /// Naechster arbeitsplatz.
        /// </summary>
        /// <value>Nummer des naechsten arbeitsplatzes.</value>
        public Dictionary<int,int> NaechsterArbeitsplatz
        {
            get
            {
                return this.naechsterSchritt;
            }
            set
            {
                this.naechsterSchritt = value;
            }
        }

        /// <summary>
        ///Arbetsplatznummer
        /// </summary>
        /// <value>The nummer.</value>
        public int Nummer
        {
            get
            {
                return this.nummer;
            }
        }



    }
}
