namespace StringCalculator;

public class Nd57Calculator
{
  private static readonly char[] _separators = { ',' };

  public int Add(string numbers)
  {
    if (numbers.Equals(string.Empty))
    {
      return 0;
    }

    var splitNumbers = numbers.Split(_separators);
    return splitNumbers.Sum(int.Parse);
  }
}
