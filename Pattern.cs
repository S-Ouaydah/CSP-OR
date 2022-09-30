using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CSP
{
    public class Pattern
    {
        public int count= 1;
        public String patternId;
        public List<int> cuts = new List<int>() ;
        public int stockSize;
        public int stockLeft;

//Construct
        public Pattern(int stockSize)
        {
            this.patternId = "P"+stockSize;
            this.stockSize = stockSize;
            this.stockLeft = stockSize;
        }
//Overrides
        public override bool Equals(object? obj){
            return obj is Pattern pattern &&
                   cuts.SequenceEqual(pattern.cuts);
        }

        public override int GetHashCode(){
            return HashCode.Combine(patternId, cuts, stockLeft);
        }
        public override string ToString()
        {
            string planDesc = '{'+patternId+" numPlaceholder} ( ";
            foreach (var cut in cuts)
            {
                // planDesc += '{'+cut.id+','+cut.size+'}';
                planDesc += cut+" ";
            }
            planDesc += $") waste:{stockLeft} | X{count}";
            return planDesc;
        }
//Methods
        public void addCut(int cut){
            if (cut<=stockLeft)
            {
            cuts.Add(cut);                
            stockLeft-=cut;
            }
            else
            {
                throw new ArgumentOutOfRangeException();
            }
        }
        
        public int getWaste(){
            return stockLeft * count;
        }

//Class Methods (utils)
        public static List<Pattern> joinSimilarPatterns(List<Pattern> patterns){
            List<Pattern> pats = new List<Pattern>();
            foreach (Pattern pat in patterns)
            {
                if (!pats.Any()){
                    pats.Add(pat);
                }
                else{
                    bool found = false;
                    foreach (Pattern newpat in pats)
                    {
                        if (pat.Equals(newpat))
                        {
                            found = true;
                            newpat.count++;
                        }
                    }    
                    if(!found) pats.Add(pat);
                }
            }
            return pats;
        }
        public static int getTotalWaste(List<Pattern> patterns){
            int total = 0;
            patterns.ForEach(pat => total += pat.getWaste());
            return total;
        }
    }
}