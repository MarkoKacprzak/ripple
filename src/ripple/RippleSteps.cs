using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using FubuCore;
using System.Linq;

namespace ripple
{
    public interface IRippleStep
    {
        void Execute(IRippleRunner runner);
    }

    public interface IRippleRunner
    {
        void BuildSolution(Solution solution);
        void CopyFiles(FileCopyPlan plan);
        void CleanDirectory(string directory);
    }

    public interface ICommandRunner
    {
        
    }

    public class RippleRunner : IRippleRunner
    {
        public RippleRunner(bool fast, IFileSystem system, ICommandRunner runner)
        {
        }

        public void BuildSolution(Solution solution)
        {
            throw new NotImplementedException();
        }

        public void CopyFiles(FileCopyPlan plan)
        {
            throw new NotImplementedException();
        }

        public void CleanDirectory(string directory)
        {
            throw new NotImplementedException();
        }
    }

    public class RipplePlan : IEnumerable<IRippleStep>
    {
        private readonly IList<IRippleStep> _steps = new List<IRippleStep>();

        public RipplePlan(IEnumerable<Solution> solutions)
        {
            guardCondition(solutions);

            var queue = new Queue<Solution>(solutions); 

            // first step
            _steps.Add(new BuildSolution(queue.Dequeue()));

            while (queue.Any())
            {
                var solution = queue.Dequeue();

                var nugets = solution
                    .NugetDependencies()
                    .Where(x => solutions.Contains(x.Publisher))
                    .OrderBy(x => x.Name)
                    .Select(x => new MoveNugetAssemblies(x, solution));

                _steps.AddRange(nugets);

                _steps.Add(new BuildSolution(solution));
            }
        }

        private static void guardCondition(IEnumerable<Solution> solutions)
        {
            if (solutions.Count() < 2)
            {
                throw new InvalidOperationException("Cannot execxute a ripple with less than 2 solutions.  It's just plain silly");
            }
        }

        public void Execute(IRippleRunner runner)
        {
            _steps.Each(x => x.Execute(runner));
        }

        public IEnumerator<IRippleStep> GetEnumerator()
        {
            return _steps.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }


    // This thing will create a FileCopyPlan
    public class MoveNugetAssemblies : IRippleStep
    {
        private readonly NugetSpec _nuget;
        private readonly Solution _destination;
        
        
        // TODO -- clean out the target lib stuff here.
        public MoveNugetAssemblies(NugetSpec nuget, Solution destination)
        {
            _nuget = nuget;
            _destination = destination;
        }

        public bool Equals(MoveNugetAssemblies other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other._nuget, _nuget) && Equals(other._destination, _destination);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof (MoveNugetAssemblies)) return false;
            return Equals((MoveNugetAssemblies) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((_nuget != null ? _nuget.GetHashCode() : 0)*397) ^ (_destination != null ? _destination.GetHashCode() : 0);
            }
        }

        public void Execute(IRippleRunner runner)
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            return string.Format("Move {0} to {1}", _nuget, _destination);
        }


    }

    public class BuildSolution : IRippleStep
    {
        private readonly Solution _solution;

        public BuildSolution(Solution solution)
        {
            _solution = solution;
        }

        public Solution Solution
        {
            get { return _solution; }
        }

        public bool Equals(BuildSolution other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other._solution, _solution);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof (BuildSolution)) return false;
            return Equals((BuildSolution) obj);
        }

        public override int GetHashCode()
        {
            return (_solution != null ? _solution.GetHashCode() : 0);
        }

        public void Execute(IRippleRunner runner)
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            return string.Format("Build {0}", _solution);
        }
    }


    public class FileCopyPlan
    {
        private readonly IList<FileCopyRequest> _requests = new List<FileCopyRequest>();


    }

    public class FileCopyRequest
    {
        public FileSet Matching { get; set; }
        public string From { get; set; }
        public string To { get; set; }

        public bool Equals(FileCopyRequest other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other.Matching, Matching) && Equals(other.From, From) && Equals(other.To, To);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof(FileCopyRequest)) return false;
            return Equals((FileCopyRequest)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int result = (Matching != null ? Matching.GetHashCode() : 0);
                result = (result * 397) ^ (From != null ? From.GetHashCode() : 0);
                result = (result * 397) ^ (To != null ? To.GetHashCode() : 0);
                return result;
            }
        }

        public override string ToString()
        {
            return string.Format("Matching: {0}, From: {1}, To: {2}", Matching, From, To);
        }
    }

}