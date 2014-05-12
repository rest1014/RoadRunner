using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace Tool
{
  public  class Produktionsplanung
    {

      DataContainer cont;
      bool aufgelost = false;

      public Produktionsplanung()
      {
          this.cont = DataContainer.Instance;
      }

      /// <summary>
      /// löst die Prognosen für Produkte berechnet die benötigten E und Kteile
      /// </summary>
      public void Aufloesen()
      {
          if (this.cont == null)
          {
              this.cont = DataContainer.Instance;
          }

          // Geht die Endprodukte P1, P2, P3 durch und ermittelt den Bedarf für die aktuelle Periode und die 3 Planperioden. 
          // Anschließend wird der Bedarf in Attributen von Teil hinterlegt
        this.RekursAufloesen(this.cont.GetTeil(1)as ETeil);
        this.RekursAufloesen(this.cont.GetTeil(2)as ETeil);
        this.RekursAufloesen(this.cont.GetTeil(3)as ETeil);
        aufgelost = true;
       
      }

      // setzt die Prognosen auf Null, damit bei zwei maligem berechnen die Werte sich nicht verdoppeln
      public void ClearData()
      {
          if (this.cont == null)
          {
              this.cont = DataContainer.Instance;
          }
          foreach (Kaufteil teil in cont.KaufteilList)
          {
              teil.VerbrauchAktuell = 0;
              teil.VerbrauchPrognose1 = 0;
              teil.VerbrauchPrognose2 = 0;
              teil.VerbrauchPrognose3 = 0;
          }
          foreach (ETeil teil in cont.ETeilList)
          {
              teil.VerbrauchAktuell = 0;
              teil.VerbrauchPrognose1 = 0;
              teil.VerbrauchPrognose2 = 0;
              teil.VerbrauchPrognose3 = 0;
          }
      }


      /// <summary>
      /// Rekursive Prozedur zum iterieren über die Zusammensetzung der Teile
      /// </summary>
      /// <param name="teil">The teil.</param>
      private void RekursAufloesen(ETeil teil)
      { 
       foreach (KeyValuePair<Teil,int> kvp in teil.Zusammensetzung)
        {
            if (kvp.Key.Nummer == 26) 
            { }

            /*kvp.Key.VerbrauchAktuell += kvp.Value * teil.VerbrauchAktuell;
            kvp.Key.VerbrauchPrognose1 += kvp.Value * teil.VerbrauchPrognose1;
            kvp.Key.VerbrauchPrognose2 += kvp.Value * teil.VerbrauchPrognose2;
            kvp.Key.VerbrauchPrognose3 += kvp.Value * teil.VerbrauchPrognose3;
           */
            if (kvp.Key is ETeil)
            {

                this.RekursAufloesen(kvp.Key as ETeil);
            }
            
        }
        return;
      }

      /// <summary>
      /// geht die ETeile durch und ermittelt die Fertigungsebene, d.h. Kategorie 1  sind P1, P2, P3, Kat 2 = Teile vom Endprodukt, Kat3 = Teile von Teilen
      /// </summary>
      private void KategorieBestimmen()
      {
      	//Kategorie 1 = Fahrraeder
          (cont.GetTeil(1) as ETeil).Kategorie = 1;
          (cont.GetTeil(2) as ETeil).Kategorie = 1;
          (cont.GetTeil(3) as ETeil).Kategorie = 1;
          foreach (ETeil teil in cont.ETeilList)
          { 
            if(teil.Kategorie==0)
            {
                if (teil.IstTeilVon.Contains(cont.GetTeil(1) as ETeil) || teil.IstTeilVon.Contains(cont.GetTeil(2) as ETeil) || teil.IstTeilVon.Contains(cont.GetTeil(3) as ETeil))
                {
                	//Kategorie 2 = Teile von Fahrraedern
                    teil.Kategorie = 2;
                }
                else
                {
                	//Kategorie 3 = Teil von Teil
                    teil.Kategorie = 3;
                }
            }
          }
         
      }


      public void Planen()
      {
          if (this.cont == null)
          {
              this.cont = DataContainer.Instance;
          }
          this.KategorieBestimmen();

          if (!aufgelost)
          {
              this.Aufloesen();
          }

          this.PrimaereProduktionsplanung();
          this.Lagerendwert();
          
          if (cont.UeberstundenErlaubt)
          {
              this.SetUeberstunde();
          }
          else
          {
              this.AnpassungMengeAnZeit();
          }
          Dictionary<int, int[]> tmp =new Dictionary<int,int[]>();
          foreach (Arbeitsplatz arbPl in this.cont.ArbeitsplatzList)
          {
              this.ReihenfolgeAnpassung(arbPl, ref tmp);
          }
          this.ReihenfolgePart2(tmp);
      }

      int c = 0;
      void test(ETeil teil)
      {
          foreach (KeyValuePair<Teil, int> kvp in teil.zusammensetzung)
          {
              if (kvp.Key.Nummer == 17 || kvp.Key.Nummer == 16 || kvp.Key.Nummer == 26)
              {
                  //TODO: hier noch was überlegen, damit bei Lagerstand / 3 mit Rest eine Ausgleichseinheit addiert wird!
                  int a = 5 / 3;

                  ETeil eTemp = kvp.Key as ETeil;
                  kvp.Key.VerbrauchAktuell += kvp.Value * teil.Produktionsmenge - kvp.Key.Lagerstand / 3 + kvp.Key.Pufferwert / 3 - eTemp.InWartschlange;

                  if (c < 4 && kvp.Key.Lagerstand % 3 != 0)
                  {
                      kvp.Key.VerbrauchAktuell += 1;
                      c++;
                  }
              }
              else
              {
                  if (kvp.Key is Kaufteil)
                  {
                      /*if (kvp.Key.Nummer == 42) //value ist 1 und nicht 7, wieso???
                      {
                          Console.Write("ETeil:" + teil.Nummer + " (" + teil.Produktionsmenge + "x)  |  " + kvp.Value + " * " + teil.Produktionsmenge + " = ");
                      }*/
                      //kvp.Key.VerbrauchAktuell += kvp.Key.Lagerstand - kvp.Value * teil.Produktionsmenge + kvp.Key.Pufferwert;
                      kvp.Key.VerbrauchAktuell += kvp.Value * teil.Produktionsmenge;

                      kvp.Key.VerbrauchPrognose1 += kvp.Value * teil.VerbrauchPrognose1;
                      kvp.Key.VerbrauchPrognose2 += kvp.Value * teil.VerbrauchPrognose2;
                      kvp.Key.VerbrauchPrognose3 += kvp.Value * teil.VerbrauchPrognose3;

                      /*if (kvp.Key.Nummer == 42)
                      {
                          Console.WriteLine(teil.Produktionsmenge*kvp.Value+"  ("+kvp.Key.VerbrauchAktuell+")");
                      }*/
                  }
                  else
                  {//ETeile - TODO: warum von der Prodmenge den Lagerstand abziehen

                      ETeil e = kvp.Key as ETeil;
                      e.VerbrauchAktuell += kvp.Value * teil.Produktionsmenge - e.Lagerstand + e.Pufferwert - e.InWartschlange;
                  }
              }

              if(kvp.Key is ETeil)
              {
                  /*if (kvp.Key.Nummer == 51 ||kvp.Key.Nummer == 16)
                  {
                  }*/

                  ETeil eteil = kvp.Key as ETeil;

                  if (eteil.VerbrauchAktuell < 0) {
                    eteil.Produktionsmenge = 0;
                  } else {
                    eteil.Produktionsmenge = eteil.VerbrauchAktuell;
                  }

                  eteil.VerbrauchPrognose1 += kvp.Value * teil.VerbrauchPrognose1;
                  eteil.VerbrauchPrognose2 += kvp.Value * teil.VerbrauchPrognose2;
                  eteil.VerbrauchPrognose3 += kvp.Value * teil.VerbrauchPrognose3;

                  test(eteil);
              }
          }
      }

      private void Lagerendwert()
      {
          foreach (Kaufteil kteil in this.cont.KaufteilList)
          {
              cont.minLagerwertEnd += (kteil.Lagerstand + kteil.MinBestellung.Bestellmenge - kteil.VerbrauchAktuell) * kteil.Lagerpreis;

              cont.normalLagerwertEnd += (kteil.Lagerstand + kteil.NormalBestellung.Bestellmenge - kteil.VerbrauchAktuell) * kteil.Lagerpreis;

              cont.maxLagerwertEnd += (kteil.Lagerstand + kteil.MaxBestellung.Bestellmenge - kteil.VerbrauchAktuell) * kteil.Lagerpreis;

              cont.lagerwertStart += kteil.Lagerstand * kteil.Lagerpreis;
          }

          Console.WriteLine();

          foreach (ETeil eteil in this.cont.ETeilList)
          {
              double wert = eteil.Pufferwert * eteil.Lagerpreis;
              cont.minLagerwertEnd += wert;
              cont.normalLagerwertEnd += wert;
              cont.maxLagerwertEnd += wert;

              cont.lagerwertStart += eteil.Lagerstand * eteil.Lagerpreis;
          }
      }

      //TODO: Berücksichtung von Warteschlangen?!?
      //Berechnung: ProdMenge = aktuellerVerbrauch - Lagerbestand + Puffer
      private void PrimaereProduktionsplanung()
      {
          ETeil p1 = this.cont.ETeilList[0];
          p1.Produktionsmenge = p1.VerbrauchAktuell - p1.Lagerstand + p1.Pufferwert - p1.InWartschlange;
          test(p1);

          ETeil teil16 = cont.GetTeil(16) as ETeil;
          ETeil teil17 = cont.GetTeil(17) as ETeil;

          ETeil teil16tmp = new ETeil(16, "");
          ETeil teil17tmp = new ETeil(17, "");

          teil16tmp.VerbrauchAktuell = teil16.VerbrauchAktuell;
          teil16tmp.VerbrauchPrognose1 = teil16.VerbrauchPrognose1;
          teil16tmp.VerbrauchPrognose2 = teil16.VerbrauchPrognose2;
          teil16tmp.VerbrauchPrognose3 = teil16.VerbrauchPrognose3;
          teil16.VerbrauchAktuell = 0;
          teil16.VerbrauchPrognose1 = 0;
          teil16.VerbrauchPrognose2 = 0;
          teil16.VerbrauchPrognose3 = 0;

          teil17tmp.VerbrauchAktuell = teil17.VerbrauchAktuell;
          teil17tmp.VerbrauchPrognose1 = teil17.VerbrauchPrognose1;
          teil17tmp.VerbrauchPrognose2 = teil17.VerbrauchPrognose2;
          teil17tmp.VerbrauchPrognose3 = teil17.VerbrauchPrognose3;
          teil17.VerbrauchAktuell = 0;
          teil17.VerbrauchPrognose1 = 0;
          teil17.VerbrauchPrognose2 = 0;
          teil17.VerbrauchPrognose3 = 0;

          ETeil p2 = this.cont.ETeilList[1];
          p2.Produktionsmenge = p2.VerbrauchAktuell - p2.Lagerstand + p2.Pufferwert - p2.InWartschlange;
          test(p2);

          teil16tmp.VerbrauchAktuell += teil16.VerbrauchAktuell;
          teil16tmp.VerbrauchPrognose1 += teil16.VerbrauchPrognose1;
          teil16tmp.VerbrauchPrognose2 += teil16.VerbrauchPrognose2;
          teil16tmp.VerbrauchPrognose3 += teil16.VerbrauchPrognose3;
          teil16.VerbrauchAktuell = 0;
          teil16.VerbrauchPrognose1 = 0;
          teil16.VerbrauchPrognose2 = 0;
          teil16.VerbrauchPrognose3 = 0;

          teil17tmp.VerbrauchAktuell += teil17.VerbrauchAktuell;
          teil17tmp.VerbrauchPrognose1 += teil17.VerbrauchPrognose1;
          teil17tmp.VerbrauchPrognose2 += teil17.VerbrauchPrognose2;
          teil17tmp.VerbrauchPrognose3 += teil17.VerbrauchPrognose3;
          teil17.VerbrauchAktuell = 0;
          teil17.VerbrauchPrognose1 = 0;
          teil17.VerbrauchPrognose2 = 0;
          teil17.VerbrauchPrognose3 = 0;

          ETeil p3 = this.cont.ETeilList[2];
          p3.Produktionsmenge = p3.VerbrauchAktuell - p3.Lagerstand + p3.Pufferwert - p3.InWartschlange;
          test(p3);

          teil16.VerbrauchAktuell += teil16tmp.VerbrauchAktuell;
          teil16.VerbrauchPrognose1 += teil16tmp.VerbrauchPrognose1;
          teil16.VerbrauchPrognose2 += teil16tmp.VerbrauchPrognose2;
          teil16.VerbrauchPrognose3 += teil16tmp.VerbrauchPrognose3;

          teil17.VerbrauchAktuell += teil17tmp.VerbrauchAktuell;
          teil17.VerbrauchPrognose1 += teil17tmp.VerbrauchPrognose1;
          teil17.VerbrauchPrognose2 += teil17tmp.VerbrauchPrognose2;
          teil17.VerbrauchPrognose3 += teil17tmp.VerbrauchPrognose3;

          /*foreach (Kaufteil kteil in this.cont.KaufteilList)
          {
              if (kteil.Nummer == 42)
              {
                  //kteil.VerbrauchAktuell += 600;
              }
              //kteil.VerbrauchAktuell = kteil.Lagerstand - kteil.VerbrauchAktuell + kteil.Pufferwert;
          }*/
      }


      public String Nachpruefen(Teil teil, int mengeNeu)
      {
          StringBuilder sb = new StringBuilder();
          foreach (KeyValuePair<Teil, int> kvp in (teil as ETeil).Zusammensetzung)
          {
              if (kvp.Key is Kaufteil)
              {
                  if (kvp.Key.Lagerstand + (kvp.Key as Kaufteil).ErwarteteBestellung < kvp.Value * mengeNeu)
                  {
                      sb.Append(string.Format("Nicht genug Kaufteile (nr. {0}) um die manuell gesetzte Menge({1}) an  Teil {2} herzustellen", kvp.Key.Nummer, mengeNeu, teil.Nummer));
                      sb.Append(Environment.NewLine);
                  }
              }
          }
          return sb.ToString();
      }

      /// <summary>
      /// Setzt die benötigten Überstunden und doppelschichten 
      /// </summary>
      /// <returns></returns>
	  //TODO wo findet der Aufruf statt, da zu diesem Zeitpunkt die benötigteZeit bekannt sein muss
      private bool SetUeberstunde()
      {
          foreach (Arbeitsplatz arbl in this.cont.ArbeitsplatzList)
          {
              if (arbl.BenoetigteZeit + arbl.Ruestzeit > arbl.ZuVerfuegungStehendeZeit)
              {
                  arbl.AddUeberMinute(Convert.ToInt32(arbl.BenoetigteZeit + arbl.Ruestzeit - arbl.ZuVerfuegungStehendeZeit) / 5);
              }
          }

          return true;
      }

      /// <summary>
      /// findet die Reihenfolge für einen Aebreitsplatz
      /// </summary>
      /// <param name="arb">Arbeitsplatz objekt</param>
      /// <param name="tmp">temporäres Dictionary</param>
      private void ReihenfolgeAnpassung(Arbeitsplatz arb,ref Dictionary<int, int[]> tmp)
      {
          int lastPos;
          int counter=0;
          bool lastposChgd;
          bool notInserted;
          if (!tmp.ContainsKey(arb.Nummer))
          {
              tmp[arb.Nummer] = new int[arb.HergestelteTeile.Count];
              lastPos = arb.HergestelteTeile.Count - 1;
              foreach (ETeil hergestellt in arb.HergestelteTeile) // alle Teile die an diesem Arbeitsplatz hergestellt werden
              {
                  lastposChgd = false;
                  notInserted = true;
                  foreach (KeyValuePair<Teil, int> bestandTeil in hergestellt.Zusammensetzung) //die Teile aus dennen sich die hergestellten Teile Zusammensetzen
                  {
                      if (bestandTeil.Key is ETeil)
                      {
                          if (bestandTeil.Key.Lagerstand - bestandTeil.Key.VerbrauchAktuell < 0) // Falls Verbrauch höher Lagerstand dieses Teil am Arbeitsplatz bevorzugen
                          {
                              tmp[arb.Nummer][lastPos] = hergestellt.Nummer;
                              lastposChgd = true;
                              notInserted = false;
                          }
                          if (bestandTeil.Key.Lagerstand - bestandTeil.Key.VerbrauchAktuell >= 0) // Falls Verbrauch kleiner/gleich Lagerstand
                          {
                              bool präferenz = false;
                              foreach (ETeil nachfolger in (bestandTeil.Key as ETeil).IstTeilVon) // Teile die von ersten Teil abhängig sind prüfen
                              {
                                  if (nachfolger.Lagerstand - nachfolger.VerbrauchAktuell < 0)
                                  {
                                      präferenz = true; // Diese Teile bevorzugen da nicht mehr vorhanden
                                      break;
                                  }
                              }
                              if (präferenz)
                              {
                                  int c = tmp[arb.Nummer][0];
                                  tmp[arb.Nummer][0] = hergestellt.Nummer;
                                  tmp[arb.Nummer][counter] = c;
                                  notInserted = false;

                              }
                              else
                              {
                                  tmp[arb.Nummer][counter] = hergestellt.Nummer;
                                  notInserted = false;

                              }
                          }
                      }
                  }//foreach KeyValuePair<Teil,int>
                  if (notInserted)
                  {
                      tmp[arb.Nummer][counter] = hergestellt.Nummer;
                  }
                  if (lastposChgd)
                  {
                      lastPos--;
                  }
                  else
                  {
                      counter++;
                  }
              }//foreach Eteil
          }//if
      }

      private void AnpassungMengeAnZeit()
      {
          double diff = 0;
          int sumProd = 0;
          double zwischenWert = 0;
          int val;

          foreach (Arbeitsplatz arbl in this.cont.ArbeitsplatzList)
          {
              sumProd = 0;
              diff = arbl.ZuVerfuegungStehendeZeit - arbl.BenoetigteZeit - arbl.Ruestzeit;
              if (diff<0)
              {
                  foreach (ETeil teil in arbl.HergestelteTeile)
                  {
                      sumProd += teil.Produktionsmenge;
                  }

                  foreach (ETeil teil in arbl.HergestelteTeile)
                  {
                      zwischenWert = Convert.ToInt32((-diff / teil.Produktionsmenge) * sumProd);
                      val =Convert.ToInt32(Math.Round(zwischenWert / arbl.WerkZeitJeStk[teil.Nummer]));
                      if (val < teil.Produktionsmenge)
                      {
                          teil.Produktionsmenge -= val;
                      }
                      else
                      { 
                      
                      }
                  }
              }
          }
        
      }

      /// <summary>
      /// wenn ein Wert bereits eingereiht ist in die Reihenfolge dann wird die Position zurückgegeben ansonsten -1
      /// </summary>
      /// <param name="nr">The nr.</param>
      /// <returns></returns>
      private int ReihenfolgeContains(int nr)
      {
          int pos = 0;
          foreach (int i in cont.Reihenfolge)
          {
              if (i == nr)
              {
                  return pos;
              }
              pos++;
          }
          return -1;
      }

      private void ReihenfolgePart2(Dictionary<int,int[]>tmp)
      {
          int counter = 0;
          int oldPos = 0;
          
          cont.Reihenfolge = new int[cont.ETeilList.Count];
          foreach (KeyValuePair<int, int[]> abPl in tmp)
          {
              foreach (int teilNr in abPl.Value)
              {
                  if ((this.cont.GetTeil(teilNr) as ETeil).BenutzteArbeitsplaetze.Count == 1)
                  {		
                  	if(cont.Reihenfolge.Length >= counter + 1) {
                      cont.Reihenfolge[counter] = teilNr;
                      counter++;
                  	}
                  }
                  else
                  {
                      oldPos = this.ReihenfolgeContains(teilNr);
                      if (oldPos == -1)
                      {
                      	if(cont.Reihenfolge.Length >= counter + 1) {
                          cont.Reihenfolge[counter] = teilNr;
                          counter++;
                      	}
                      }

                  }
              }
          }
      }

    }
}
