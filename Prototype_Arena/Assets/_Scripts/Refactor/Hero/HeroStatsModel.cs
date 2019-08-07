namespace _Scripts.Refactor.Hero
{
    public class HeroStatsModel
    {
        private int _healthPoints;
        private int _attackDamage;
        private int _initiative;
        private int _armorValue;
        private int _xPositionInGrid;
        private int _yPositionInGrid;

        public int HealthPoints
        {
            get { return _healthPoints; }
        }

        public int AttackDamage
        {
            get { return _attackDamage; }
        }

        public int Initiative
        {
            get { return _initiative; }
        }

        public int ArmorValue
        {
            get { return _armorValue; }
        }

        public int XPositionInGrid
        {
            get { return _xPositionInGrid; }
        }

        public int YPositionInGrid
        {
            get { return _yPositionInGrid; }
        }

        public void SetHealthPoints(int healthPoints)
        {
            _healthPoints = healthPoints;
        }

        public void SetAttackDamage(int attackDamage)
        {
            _attackDamage = attackDamage;
        }

        public void SetInitiative(int initiative)
        {
            _initiative = initiative;
        }

        public void SetArmorValue(int armorValue)
        {
            _armorValue = armorValue;
        }

        public void SetXPositionInGrid(int xPosition)
        {
            _xPositionInGrid = xPosition;
        }

        public void SetYPositionInGrid(int yPosition)
        {
            _yPositionInGrid = yPosition;
        }
    }
}