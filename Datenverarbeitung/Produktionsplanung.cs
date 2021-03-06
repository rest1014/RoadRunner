using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace Tool
{
  public  class Produktionsplanung
    {

      DataContainer cont;

      /// <summary>
      /// Delegatormethode
      /// </summary>
      public void Planen()
      {
          if (this.cont == null)
          {
              this.cont = DataContainer.Instance;
          }

          this.KategorieBestimmen();
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
          Dictionary<int, int[]> tmp = new Dictionary<int, int[]>();
          foreach (Arbeitsplatz arbPl in this.cont.ArbeitsplatzList)
          {
              this.ReihenfolgeAnpassung(arbPl, ref tmp);
          }
          this.ReihenfolgePart2(tmp);
      }

      public Produktionsplanung()
      {
          this.cont = DataContainer.Instance;
      }

      /// <summary>
      /// Berechnung: ProdMenge = aktuellerVerbrauch - Lagerbestand + Puffer
      /// </summary>
      private void PrimaereProduktionsplanung()
      {
          //TODO: Berücksichtung von Warteschlangen?!?
          ETeil p1 = this.cont.ETeilList[0];
          //p1.Produktionsmenge = - p1.Lagerstand + p1.Pufferwert; // +p1.Warteschlange;
          
          p1.Produktionsmenge = VerbrauchAlgorithmus( p1.VerbrauchAktuell, p1.Lagerstand, p1.Pufferwert, p1.Warteschlange);

          if(p1.Produktionsmenge > 0)
          {
              VerbrauchBerechnen(p1);
          }
         
          // Teile 16, 17 und 26 werden für alle drei Primärprodukte benötigt
          ETeil teil16 = cont.GetTeil(16) as ETeil;
          ETeil teil17 = cont.GetTeil(17) as ETeil;
          ETeil teil26 = cont.GetTeil(26) as ETeil;

          ETeil teil16tmp = new ETeil(16, "");
          ETeil teil17tmp = new ETeil(17, "");
          ETeil teil26tmp = new ETeil(26, "");

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

          teil26tmp.VerbrauchAktuell = teil26.VerbrauchAktuell;
          teil26tmp.VerbrauchPrognose1 = teil26.VerbrauchPrognose1;
          teil26tmp.VerbrauchPrognose2 = teil26.VerbrauchPrognose2;
          teil26tmp.VerbrauchPrognose3 = teil26.VerbrauchPrognose3;
          teil26.VerbrauchAktuell = 0;
          teil26.VerbrauchPrognose1 = 0;
          teil26.VerbrauchPrognose2 = 0;
          teil26.VerbrauchPrognose3 = 0;

          ETeil p2 = this.cont.ETeilList[1];
          //p2.Produktionsmenge = p2.VerbrauchAktuell - p2.Lagerstand + p2.Pufferwert;// -p2.InWarteschlange;
          p2.Produktionsmenge = VerbrauchAlgorithmus(p2.VerbrauchAktuell, p2.Lagerstand, p2.Pufferwert, p2.Warteschlange);
          
          if (p2.Produktionsmenge > 0)
          {
              VerbrauchBerechnen(p2);
          }

          // Teile 16, 17 und 26 werden für alle drei Primärprodukte benötigt
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

          teil26tmp.VerbrauchAktuell += teil26.VerbrauchAktuell;
          teil26tmp.VerbrauchPrognose1 += teil26.VerbrauchPrognose1;
          teil26tmp.VerbrauchPrognose2 += teil26.VerbrauchPrognose2;
          teil26tmp.VerbrauchPrognose3 += teil26.VerbrauchPrognose3;
          teil26.VerbrauchAktuell = 0;
          teil26.VerbrauchPrognose1 = 0;
          teil26.VerbrauchPrognose2 = 0;
          teil26.VerbrauchPrognose3 = 0;

          ETeil p3 = this.cont.ETeilList[2];
          //p3.Produktionsmenge = p3.VerbrauchAktuell - p3.Lagerstand + p3.Pufferwert;// -p3.InWarteschlange;
          p3.Produktionsmenge = VerbrauchAlgorithmus(p3.VerbrauchAktuell, p3.Lagerstand, p3.Pufferwert, p3.Warteschlange);
          
          if (p3.Produktionsmenge > 0)
          {
              VerbrauchBerechnen(p3);
          }

          // Teile 16, 17 und 26 werden für alle drei Primärprodukte benötigt
          teil16.VerbrauchAktuell += teil16tmp.VerbrauchAktuell;
          teil16.VerbrauchPrognose1 += teil16tmp.VerbrauchPrognose1;
          teil16.VerbrauchPrognose2 += teil16tmp.VerbrauchPrognose2;
          teil16.VerbrauchPrognose3 += teil16tmp.VerbrauchPrognose3;

          teil17.VerbrauchAktuell += teil17tmp.VerbrauchAktuell;
          teil17.VerbrauchPrognose1 += teil17tmp.VerbrauchPrognose1;
          teil17.VerbrauchPrognose2 += teil17tmp.VerbrauchPrognose2;
          teil17.VerbrauchPrognose3 += teil17tmp.VerbrauchPrognose3;

          teil26.VerbrauchAktuell += teil26tmp.VerbrauchAktuell;
          teil26.VerbrauchPrognose1 += teil26tmp.VerbrauchPrognose1;
          teil26.VerbrauchPrognose2 += teil26tmp.VerbrauchPrognose2;
          teil26.VerbrauchPrognose3 += teil26tmp.VerbrauchPrognose3;
      }

      /// <summary>
      /// setzt die Prognosen auf Null, damit bei zwei maligem berechnen die Werte sich nicht verdoppeln
      /// </summary>
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

      int c = 0;
      private void VerbrauchBerechnen(ETeil teil)
      {
          foreach (KeyValuePair<Teil, int> kvp in teil.zusammensetzung)
          {
              if (kvp.Key.Nummer == 17 || kvp.Key.Nummer == 16 || kvp.Key.Nummer == 26)
              {
                  //Math.Ceiling berechnet ganzzahlig -> 7.2 wird zu 8
                  int Lagerbestand = Convert.ToInt32(Math.Ceiling(kvp.Key.Lagerstand / 3.0));
                  int Pufferwert = Convert.ToInt32(Math.Ceiling(kvp.Key.Pufferwert / 3.0));

                  ETeil eTemp = kvp.Key as ETeil;
                  int verbrauch = kvp.Value * teil.Produktionsmenge;
                  kvp.Key.VerbrauchAktuell += VerbrauchAlgorithmus(verbrauch, Lagerbestand, Pufferwert, eTemp.InWarteschlange);
                  //kvp.Key.VerbrauchAktuell += (kvp.Value * teil.Produktionsmenge) - Lagerbestand + Pufferwert - eTemp.InWarteschlange;
                  
                  //TODO: was hat der Zähler c zu bedeutet?
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
                      //Lagerwert wird in PrimaerPlanung abgezogen
                      //kvp.Key.VerbrauchAktuell += (kvp.Value * teil.Produktionsmenge) - kvp.Key.Lagerstand + kvp.Key.Pufferwert;
                      int verbrauch = kvp.Value * teil.Produktionsmenge;
                      kvp.Key.VerbrauchAktuell += VerbrauchAlgorithmus(verbrauch, kvp.Key.Lagerstand, kvp.Key.Pufferwert, kvp.Key.Warteschlange);

                      if(kvp.Key.Warteschlange != 0)
                      {
                          Console.WriteLine(kvp.Key.Warteschlange);
                      }

                      kvp.Key.VerbrauchPrognose1 += kvp.Value * teil.VerbrauchPrognose1;
                      kvp.Key.VerbrauchPrognose2 += kvp.Value * teil.VerbrauchPrognose2;
                      kvp.Key.VerbrauchPrognose3 += kvp.Value * teil.VerbrauchPrognose3;

                  }
                  else
                  {
                      ETeil e = kvp.Key as ETeil;
                      int verbrauch = kvp.Value * teil.Produktionsmenge;
                      e.VerbrauchAktuell += VerbrauchAlgorithmus(verbrauch, e.Lagerstand, e.Pufferwert, e.InWarteschlange);
                      //e.VerbrauchAktuell += (kvp.Value * teil.Produktionsmenge) - e.Lagerstand + e.Pufferwert - e.InWarteschlange;
                  }
              }

              if(kvp.Key is ETeil)
              {
                  //TODO: Warum hier ein if? Sonderbehandlung notwendig?
                  /*if (kvp.Key.Nummer == 51 ||kvp.Key.Nummer == 16)
                  {
                   * 
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

                  VerbrauchBerechnen(eteil);
              }
          }
      }

      /// <summary>
      /// Submethode von VerbrauchBerechnen, damit Algorithmus zentral geändert werden kann
      /// </summary>
      private int VerbrauchAlgorithmus(int Verbrauch, int Lagerbestand, int Pufferwert, int Warteschlange)
      {
          int notwendigeMenge;

          notwendigeMenge = Verbrauch - Lagerbestand - Warteschlange + Pufferwert;

          if (Warteschlange > 0)
          {
              Console.WriteLine();
          }


          if (notwendigeMenge > 0)
          {
              return notwendigeMenge;
          }
          else
          {
              return 0;
          }


          // return (Value * Produktionsmenge) - Lagerbestand + Pufferwert; // -Warteschlange;
          //Warteschlange wird beim Einlesen vom Lagerbestand abgezogen. ETeil.InWarteschlange wird in der Methode AddWarteschlange befüllt
      }

      /// <summary>
      /// 
      /// </summary>
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

      /// <summary>
      /// prüft nach manueller Änderung der Menge, aktuell nicht implementiert
      /// </summary>
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
                      //Teil ist ETeil und wir produziert
                      {
                          if ((bestandTeil.Key.Lagerstand - bestandTeil.Key.VerbrauchAktuell) < 0) // Falls Verbrauch höher Lagerstand dieses Teil am Arbeitsplatz bevorzugen
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
          foreach (Arbeitsplatz arbl in this.cont.ArbeitsplatzList)
          {
              double diff = arbl.ZuVerfuegungStehendeZeit - arbl.BenoetigteZeit - arbl.Ruestzeit;
              int sumProdMenge = 0;
              if (diff < 0)
              {
                  foreach (ETeil teil in arbl.HergestelteTeile)
                  {
                      sumProdMenge += teil.Produktionsmenge;
                  }

                  foreach (ETeil teil in arbl.HergestelteTeile)
                  {
                      double zwischenWert = Convert.ToInt32((-diff / teil.Produktionsmenge) * sumProdMenge);
                      int val = Convert.ToInt32(Math.Ceiling(zwischenWert / arbl.WerkZeitJeStk[teil.Nummer]));
                      if (val < teil.Produktionsmenge)
                      {
                          teil.Produktionsmenge -= val;
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
          
          cont.Reihenfolge = new int[cont.ETeilList.Count*2];
          foreach (KeyValuePair<int, int[]> abPl in tmp)
          {
              foreach (int teilNr in abPl.Value)
              {
                  if ((this.cont.GetTeil(teilNr) as ETeil).BenutzteArbeitsplaetze.Count == 1)
                  {		
                  	if(cont.Reihenfolge.Length >= counter + 1) {
                      cont.Reihenfolge[counter] = teilNr;
                      cont.Reihenfolge[29 + counter] = teilNr;
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
                          cont.Reihenfolge[29 + counter] = teilNr;
                          counter++;
                      	}
                      }

                  }
              }
          }
      }

    }
}
